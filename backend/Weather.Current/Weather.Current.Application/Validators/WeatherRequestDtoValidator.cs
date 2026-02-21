using FluentValidation;
using Weather.Current.Application.DTOs;

namespace Weather.Current.Application.Validators;

public class WeatherRequestDtoValidator : AbstractValidator<WeatherRequestDto>
{
    public WeatherRequestDtoValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MinimumLength(2).WithMessage("City must be at least 2 characters")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");
    }
}
