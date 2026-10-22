namespace FrameRent.Domain.Exceptions;

/// <summary>
/// Exception thrown when a domain business rule is violated.
/// Intended to be handled at the API presentation layer and mapped to an HTTP 400 Bad Request response.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
