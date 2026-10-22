namespace FrameRent.Application.Common;

/// <summary>
/// Base exception for application-level operational and business failures.
/// Mapped to specific HTTP status codes by the API exception handling middleware.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }

    protected AppException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when an entity cannot be located by its identifier. Maps to HTTP 404 Not Found.
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with id '{key}' was not found.")
    {
    }
}

/// <summary>
/// Thrown when an authenticated user attempts an action outside their permissions or ownership. Maps to HTTP 403 Forbidden.
/// </summary>
public class ForbiddenAccessException : AppException
{
    public ForbiddenAccessException(string message = "You are not allowed to perform this action.")
        : base(message)
    {
    }
}

/// <summary>
/// Thrown when credentials fail verification during authentication. Maps to HTTP 401 Unauthorized.
/// </summary>
public class AuthenticationFailedException : AppException
{
    public AuthenticationFailedException(string message = "Invalid credentials.")
        : base(message)
    {
    }
}

/// <summary>
/// Thrown when application-level business constraints or invariants are violated. Maps to HTTP 400 Bad Request.
/// </summary>
public class ValidationAppException : AppException
{
    public ValidationAppException(string message)
        : base(message)
    {
    }
}
