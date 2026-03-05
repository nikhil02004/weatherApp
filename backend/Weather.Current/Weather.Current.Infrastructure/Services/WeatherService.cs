using AutoMapper;
using Weather.Current.Application.DTOs;
using Weather.Current.Application.Interfaces;

namespace Weather.Current.Infrastructure.Services;

public class WeatherService(IWeatherRepository repository, IMapper mapper) : IWeatherService
{
    public async Task<WeatherResponseDto?> GetWeatherByCityAsync(string city)
    {
        var weatherResponse = await repository.GetByCity(city);

        if (weatherResponse is null)
            return null;

        return mapper.Map<WeatherResponseDto>(weatherResponse);
    }
}
