using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Weather.Current.Application.Interfaces;

namespace Weather.Current.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger) : ControllerBase
{
    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest(new { message = "City is required" });

        var weather = await weatherService.GetWeatherByCityAsync(city);

        if (weather == null)
        {
            logger.LogWarning("Weather data not found for city: {City}", city);
            return NotFound(new { message = $"Weather data not found for city: {city}" });
        }

        return Ok(weather);
    }
}
