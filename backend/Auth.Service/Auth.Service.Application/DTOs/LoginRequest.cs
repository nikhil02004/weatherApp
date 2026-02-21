namespace Auth.Service.Application.DTOs;

public record LoginRequest(string? Username, string Password, string? Email);
