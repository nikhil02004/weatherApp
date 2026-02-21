namespace Weather.Current.Domain.Entities;

public class WeatherLocation
{
    public required string City { get; set; }
    public required string Country { get; set; }
    public string? Region { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
