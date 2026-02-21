namespace Weather.Current.Domain.Exceptions;

public class WeatherDomainException : Exception
{
    public WeatherDomainException(string message) : base(message) { }
    public WeatherDomainException(string message, Exception inner) : base(message, inner) { }
}
