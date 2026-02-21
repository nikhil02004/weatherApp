using Weather.Current.Application.DTOs;

namespace Weather.Current.Application.Interfaces;

public interface IWeatherService
{
    Task<WeatherResponseDto?> GetWeatherByCityAsync(string city);
}
