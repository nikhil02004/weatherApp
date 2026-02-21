namespace Weather.Current.Domain.Entities;

public class WeatherRequest
{
    public required string City { get; set; }
    public string? CountryCode { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
