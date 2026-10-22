using System.Security.Claims;
using FrameRent.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FrameRent.Infrastructure.Services;

/// <summary>
/// Retrieves authenticated user identity information from HttpContext.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}
