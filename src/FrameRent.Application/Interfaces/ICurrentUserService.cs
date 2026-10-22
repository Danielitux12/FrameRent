namespace FrameRent.Application.Interfaces;

/// <summary>
/// Exposes the identity and role memberships of the currently authenticated user
/// without coupling the application layer to HttpContext or ASP.NET Core web abstractions.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
