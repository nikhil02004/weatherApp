namespace Weather.Current.Domain.Entities;

public class WeatherResponse
{
    public required string City { get; set; }
    public required string Country { get; set; }
    public string? Region { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LocalTime { get; set; }
    public double TemperatureC { get; set; }
    public double WindKph { get; set; }
    public int Humidity { get; set; }
    public string? Condition { get; set; }
}
