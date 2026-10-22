using FrameRent.Application.DTOs.Auth;

namespace FrameRent.Application.Interfaces;

/// <summary>
/// Contract for authentication operations (client registration and user login).
/// Implemented in the infrastructure layer using ASP.NET Core Identity.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterClientAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
