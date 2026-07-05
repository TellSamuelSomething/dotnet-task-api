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

        var (token, expiresAt) = _authService.GenerateToken(user.Username);
        return Ok(new AuthResponse { Token = token, ExpiresAt = expiresAt });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _authService.ValidateCredentialsAsync(request.Username, request.Password);

        if (user is null)
            return Unauthorized("Invalid username or password.");

        var (token, expiresAt) = _authService.GenerateToken(user.Username);
        return Ok(new AuthResponse { Token = token, ExpiresAt = expiresAt });
    }
}
