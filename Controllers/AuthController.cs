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

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        if (!_authService.ValidateCredentials(request.Username, request.Password))
            return Unauthorized("Invalid username or password.");

        var (token, expiresAt) = _authService.GenerateToken(request.Username);

        return Ok(new AuthResponse { Token = token, ExpiresAt = expiresAt });
    }
}