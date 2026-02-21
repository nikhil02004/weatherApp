namespace Weather.Current.Application.DTOs;

public record WeatherRequestDto(string City, string? CountryCode = null);
