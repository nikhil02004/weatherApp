namespace Auth.Service.Application.DTOs;

public record AuthResponse(string Token, string Username, string? Email);
