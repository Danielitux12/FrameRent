namespace FrameRent.Application.Interfaces;

/// <summary>
/// Contract for generating JSON Web Tokens (JWT) for authenticated users.
/// Implemented in the infrastructure layer.
/// </summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(string userId, string email, string fullName, string role);
}
