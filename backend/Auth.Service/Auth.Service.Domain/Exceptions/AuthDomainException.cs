namespace Auth.Service.Domain.Exceptions;

public class AuthDomainException : Exception
{
    public AuthDomainException(string message) : base(message) { }
    public AuthDomainException(string message, Exception inner) : base(message, inner) { }
}
