using Auth.Service.Application.DTOs;
using Auth.Service.Application.Interfaces;
using Auth.Service.Domain.Entities;
using Auth.Service.Domain.Exceptions;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Auth.Service.Infrastructure.Services;

public class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IConfiguration configuration) : IAuthService
{
    public AuthResponse Register(RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Username))
            throw new AuthDomainException("Username is required");

        if (userRepository.FindByUsername(request.Username) != null)
            throw new AuthDomainException("Username already taken");

        if (userRepository.FindByEmail(request.Email) != null)
            throw new DuplicateEmailException("An account with this email already exists");

        var user = new ApplicationUser
        {
            Id           = Guid.NewGuid().ToString(),
            Username     = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email        = request.Email
        };

        userRepository.TryAddUser(user);

        return new AuthResponse(tokenService.CreateToken(user), user.Username, user.Email);
    }

    public AuthResponse Login(LoginRequest request)
    {
        var user = request.Username is not null
            ? userRepository.FindByUsername(request.Username)
            : null;
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AuthDomainException("Invalid username or password");

        return new AuthResponse(tokenService.CreateToken(user), user.Username, user.Email);
    }

    public async Task<AuthResponse> GoogleLogin(ExternalAuthRequest request)
    {
        var clientId = configuration["Google:ClientId"]!;
        var payload  = await GoogleJsonWebSignature.ValidateAsync(
            request.IdToken,
            new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });

        var user = userRepository.FindByExternalId("Google", payload.Subject);

        if (user == null)
        {
            var existingByEmail = userRepository.FindByEmail(payload.Email);
            if (existingByEmail != null && existingByEmail.ExternalProvider == null)
                throw new DuplicateEmailException(
                    "An account with this email already exists. Please log in with your username and password.");

            var username = payload.Email.Split('@')[0] + "_google";
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
        }

        return new AuthResponse(tokenService.CreateToken(user), user.Username, user.Email);
    }
}
