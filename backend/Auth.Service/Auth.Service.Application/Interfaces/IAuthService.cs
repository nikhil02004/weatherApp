using Auth.Service.Application.DTOs;

namespace Auth.Service.Application.Interfaces;

public interface IAuthService
{
    AuthResponse Register(RegisterRequest request);
    AuthResponse Login(LoginRequest request);
}
