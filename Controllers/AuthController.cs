using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TaskAPI.DTOs;
using TaskAPI.Services;

namespace TaskAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request.Username, request.Password);

        if (user is null)
            return Conflict("Username is already taken.");

        var accessToken = _authService.GenerateAccessToken(user.Username);
        var refreshToken = await _authService.GenerateRefreshTokenAsync(user.Id);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _authService.ValidateCredentialsAsync(request.Username, request.Password);

        if (user is null)
            return Unauthorized("Invalid username or password.");

        var accessToken = _authService.GenerateAccessToken(user.Username);
        var refreshToken = await _authService.GenerateRefreshTokenAsync(user.Id);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var response = await _authService.RefreshAsync(request.RefreshToken);
        return response is null ? Unauthorized("Invalid or expired refresh token.") : Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request)
    {
        await _authService.RevokeAsync(request.RefreshToken);
        return NoContent();
    }
}
