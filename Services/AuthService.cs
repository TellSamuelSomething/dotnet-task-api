using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskAPI.Services;

public class AuthService
{
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    public bool ValidateCredentials(string username, string password)
    {
        return username == _config["Auth:Username"] && password == _config["Auth:Password"];
    }

    public (string token, DateTime expiresAt) GenerateToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: [new Claim(ClaimTypes.Name, username)],
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
