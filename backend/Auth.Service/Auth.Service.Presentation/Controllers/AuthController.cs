using Auth.Service.Application.DTOs;
using Auth.Service.Application.Interfaces;
using Auth.Service.Domain.Exceptions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Service.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    private readonly ILogger<AuthController> _logger = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "User {Username} registered")]
    private partial void LogUserRegistered(string username);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {Username} logged in")]
    private partial void LogUserLoggedIn(string username);

    [LoggerMessage(Level = LogLevel.Information, Message = "Google user {Email} logged in")]
    private partial void LogGoogleUserLoggedIn(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid Google token: {Message}")]
    private partial void LogInvalidGoogleToken(string message);
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = authService.Register(request);
            LogUserRegistered(response.Username);
            return Ok(response);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(ex.Message);
        }
        catch (AuthDomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = authService.Login(request);
            LogUserLoggedIn(response.Username);
            return Ok(response);
        }
        catch (AuthDomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuthRequest request)
    {
        try
        {
            var response = await authService.GoogleLogin(request);
            LogGoogleUserLoggedIn(response.Email!);
            return Ok(response);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(ex.Message);
        }
        catch (InvalidJwtException ex)
        {
            LogInvalidGoogleToken(ex.Message);
            return Unauthorized("Invalid Google token");
        }
    }
}
