namespace Auth.Service.Domain.Exceptions;

/// <summary>
/// Thrown when a registration or Google login is attempted with an e-mail
/// that already belongs to a different account → maps to HTTP 409 Conflict.
/// </summary>
public class DuplicateEmailException(string message) : AuthDomainException(message);
