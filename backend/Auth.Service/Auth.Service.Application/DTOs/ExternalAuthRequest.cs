namespace Auth.Service.Application.DTOs;

public record ExternalAuthRequest(string Provider, string IdToken);
