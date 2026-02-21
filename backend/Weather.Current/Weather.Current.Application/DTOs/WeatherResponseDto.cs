namespace Weather.Current.Application.DTOs;

public record WeatherResponseDto
{
    public required string City { get; set; }
    public required string Country { get; set; }
    public string? AdminRegion { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CurrentTime { get; set; }
    public double CurrentTemperature { get; set; }
    public double CurrentWindSpeed { get; set; }
    public int CurrentRelativeHumidity { get; set; }
}
