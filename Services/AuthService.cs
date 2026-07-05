using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Models;

namespace TaskAPI.Services;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public AuthService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return null;

        return VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    public async Task<User?> RegisterAsync(string username, string password)
    {
        if (await _db.Users.AnyAsync(u => u.Username == username))
            return null;

        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public string GenerateAccessToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: [new Claim(ClaimTypes.Name, username)],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(int userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        _db.RefreshTokens.Add(new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        await _db.SaveChangesAsync();
        return token;
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            return null;

        // Rotate: revoke old token, issue new pair
        stored.IsRevoked = true;
        await _db.SaveChangesAsync();

        var accessToken = GenerateAccessToken(stored.User.Username);
        var newRefreshToken = await GenerateRefreshTokenAsync(stored.UserId);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<bool> RevokeAsync(string refreshToken)
    {
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (stored is null) return false;

        stored.IsRevoked = true;
        await _db.SaveChangesAsync();
        return true;
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
