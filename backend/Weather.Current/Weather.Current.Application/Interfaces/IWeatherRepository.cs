using Weather.Current.Domain.Entities;

namespace Weather.Current.Application.Interfaces;

public interface IWeatherRepository
{
    Task<WeatherResponse?> GetByCity(string city);
}
