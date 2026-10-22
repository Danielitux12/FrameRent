namespace FrameRent.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    string UserId,
    string FullName,
    string Email,
    string Role
);
