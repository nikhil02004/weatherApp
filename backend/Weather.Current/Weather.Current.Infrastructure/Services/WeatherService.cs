using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Weather.Current.Application.DTOs;
using Weather.Current.Application.Interfaces;

namespace Weather.Current.Infrastructure.Services;

public class WeatherService(HttpClient httpClient, IConfiguration config) : IWeatherService
{
    public async Task<WeatherResponseDto?> GetWeatherByCityAsync(string city)
    {
        var apiKey = config["WeatherApi:ApiKey"];
        var apiEndpoint = config["WeatherApi:Endpoint"];
        var url = $"{apiEndpoint}?key={apiKey}&q={city}&aqi=no";

        try
        {
            var response = await httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<WeatherApiResponse>(json);

            if (data?.Location == null || data.Current == null)
                return null;

            return new WeatherResponseDto
            {
                City = data.Location.Name,
                Country = data.Location.Country,
                AdminRegion = data.Location.Region,
                Latitude = data.Location.Lat,
                Longitude = data.Location.Lon,
                CurrentTime = DateTime.Parse(data.Location.Localtime, CultureInfo.InvariantCulture),
                CurrentTemperature = data.Current.TempC,
                CurrentWindSpeed = data.Current.WindKph,
                CurrentRelativeHumidity = data.Current.Humidity
            };
        }
        catch
        {
            return null;
        }
    }

    // ── Internal models (only used by Infrastructure) ──
    private sealed class WeatherApiResponse
    {
        [JsonPropertyName("location")] public LocationData? Location { get; set; }
        [JsonPropertyName("current")]  public CurrentData?  Current  { get; set; }
    }

    private sealed class LocationData
    {
        [JsonPropertyName("name")]      public string Name      { get; set; } = default!;
        [JsonPropertyName("region")]    public string Region    { get; set; } = default!;
        [JsonPropertyName("country")]   public string Country   { get; set; } = default!;
        [JsonPropertyName("lat")]       public double Lat       { get; set; }
        [JsonPropertyName("lon")]       public double Lon       { get; set; }
        [JsonPropertyName("localtime")] public string Localtime { get; set; } = default!;
    }

    private sealed class CurrentData
    {
        [JsonPropertyName("temp_c")]    public double TempC    { get; set; }
        [JsonPropertyName("wind_kph")]  public double WindKph  { get; set; }
        [JsonPropertyName("humidity")]  public int    Humidity { get; set; }
        [JsonPropertyName("condition")] public ConditionData? Condition { get; set; }
    }

    private sealed class ConditionData
    {
        [JsonPropertyName("text")] public string Text { get; set; } = default!;
    }
}
