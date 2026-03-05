using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Weather.Current.Application.DTOs;
using Weather.Current.Application.Interfaces;

namespace Weather.Current.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController(
    IWeatherService weatherService,
    IValidator<WeatherRequestDto> validator,
    ILogger<WeatherController> logger) : ControllerBase
{
    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        var request = new WeatherRequestDto(city);
        var validation = await validator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var weather = await weatherService.GetWeatherByCityAsync(request.City);

        if (weather is null)
        {
            logger.LogWarning("Weather data not found for city: {City}", city);
            return NotFound(new { message = $"Weather data not found for city: {city}" });
        }

        return Ok(weather);
    }
}
