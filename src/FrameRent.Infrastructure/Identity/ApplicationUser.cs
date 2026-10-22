using Microsoft.AspNetCore.Identity;

namespace FrameRent.Infrastructure.Identity;

/// <summary>
/// Application user entity extending ASP.NET Core Identity with domain-specific user profile data.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
