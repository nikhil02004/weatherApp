using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Weather.Current.Application.Interfaces;
using Weather.Current.Domain.Entities;

namespace Weather.Current.Infrastructure.Repositories;

public class WeatherRepository(HttpClient httpClient, IConfiguration config) : IWeatherRepository
{
    public async Task<WeatherResponse?> GetByCity(string city)
    {
        var apiKey = config["WeatherApi:ApiKey"];
        var apiEndpoint = config["WeatherApi:Endpoint"];
        var url = $"{apiEndpoint}?key={apiKey}&q={city}&aqi=no";

        try
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<WeatherApiResponse>(json);

            if (data?.Location == null || data.Current == null)
                return null;

            return new WeatherResponse
            {
                City      = data.Location.Name,
                Country   = data.Location.Country,
                Region    = data.Location.Region,
                Latitude  = data.Location.Lat,
                Longitude = data.Location.Lon,
                LocalTime    = DateTime.Parse(data.Location.Localtime, CultureInfo.InvariantCulture),
                TemperatureC = data.Current.TempC,
                WindKph      = data.Current.WindKph,
                Humidity     = data.Current.Humidity,
                Condition    = data.Current.Condition?.Text
            };
        }
        catch
        {
            return null;
        }
    }

    // ── External API response models (infrastructure concern only) ──
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
