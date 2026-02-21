using Auth.Service.Application.DTOs;
using Auth.Service.Application.Interfaces;
using Auth.Service.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Service.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthController(
    IUserRepository userRepository,
    ITokenService tokenService,
    ILogger<AuthController> logger,
    IConfiguration configuration) : ControllerBase
{
    private readonly ILogger<AuthController> _logger = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "User {Username} registered")]
    private partial void LogUserRegistered(string username);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {Username} logged in")]
    private partial void LogUserLoggedIn(string username);

    [LoggerMessage(Level = LogLevel.Information, Message = "New Google user registered: {Email}")]
    private partial void LogNewGoogleUserRegistered(string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Google user {Email} logged in")]
    private partial void LogGoogleUserLoggedIn(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid Google token: {Message}")]
    private partial void LogInvalidGoogleToken(string message);
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Username and password required");

        if (string.IsNullOrEmpty(request.Email))
            return BadRequest("Email is required");

        if (userRepository.FindByUsername(request.Username) != null)
            return BadRequest("Username already taken");

        if (userRepository.FindByEmail(request.Email) != null)
            return BadRequest("An account with this email already exists");

        var newUser = new ApplicationUser
        {
            Id           = Guid.NewGuid().ToString(),
            Username     = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email        = request.Email
        };

        userRepository.TryAddUser(newUser);

        var token = tokenService.CreateToken(newUser);
        LogUserRegistered(newUser.Username);

        return Ok(new AuthResponse(token, newUser.Username, newUser.Email));
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Username and password required");

        var user = userRepository.FindByUsername(request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return BadRequest("Invalid username or password");

        var token = tokenService.CreateToken(user);
        LogUserLoggedIn(user.Username);

        return Ok(new AuthResponse(token, user.Username, user.Email));
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuthRequest request)
    {
        try
        {
            var clientId = configuration["Google:ClientId"]!;
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

            var user = userRepository.FindByExternalId("Google", payload.Subject);

            if (user == null)
            {
                // Check if email already belongs to a local (non-Google) account
                var existingByEmail = userRepository.FindByEmail(payload.Email);
                if (existingByEmail != null && existingByEmail.ExternalProvider == null)
                    return Conflict("An account with this email already exists. Please log in with your username and password.");

                var username = payload.Email.Split('@')[0] + "_google";

                // ensure username uniqueness if collision
                if (userRepository.FindByUsername(username) != null)
                    username = username + "_" + Guid.NewGuid().ToString("N")[..6];

                user = new ApplicationUser
                {
                    Id               = Guid.NewGuid().ToString(),
                    Username         = username,
                    PasswordHash     = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Email            = payload.Email,
                    ExternalProvider = "Google",
                    ExternalId       = payload.Subject
                };

                userRepository.TryAddUser(user);
                LogNewGoogleUserRegistered(payload.Email);
            }

            var token = tokenService.CreateToken(user);
            LogGoogleUserLoggedIn(payload.Email);

            return Ok(new AuthResponse(token, user.Username, user.Email));
        }
        catch (InvalidJwtException ex)
        {
            LogInvalidGoogleToken(ex.Message);
            return Unauthorized("Invalid Google token");
        }
    }
}
