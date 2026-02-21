namespace Auth.Service.Application.DTOs;

public record RegisterRequest(string? Username, string Password, string Email);
