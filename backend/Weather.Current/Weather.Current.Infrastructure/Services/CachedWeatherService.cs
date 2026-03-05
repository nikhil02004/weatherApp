using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Weather.Current.Application.DTOs;
using Weather.Current.Application.Interfaces;

namespace Weather.Current.Infrastructure.Services;

public class CachedWeatherService(
    WeatherService inner,
    IDistributedCache cache,
    ILogger<CachedWeatherService> logger) : IWeatherService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<WeatherResponseDto?> GetWeatherByCityAsync(string city)
    {
        var cacheKey = $"weather:{city.Trim().ToLowerInvariant()}";

        var cached = await cache.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            logger.LogInformation("Cache HIT for city: {City}", city);
            return JsonSerializer.Deserialize<WeatherResponseDto>(cached);
        }

        logger.LogInformation("Cache MISS for city: {City} — fetching from API", city);
        var result = await inner.GetWeatherByCityAsync(city);

        if (result is not null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options);
            logger.LogInformation("Cached weather data for city: {City} (expires in 10 min)", city);
        }

        return result;
    }
}
