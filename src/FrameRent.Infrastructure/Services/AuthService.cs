using FrameRent.Application.Common;
using FrameRent.Application.DTOs.Auth;
using FrameRent.Application.Interfaces;
using FrameRent.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace FrameRent.Infrastructure.Services;

/// <summary>
/// Implements user registration and authentication using ASP.NET Core Identity and JWT token issuance.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterClientAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new ValidationAppException("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new ValidationAppException($"User registration failed: {errors}");
        }

        // Anyone registering through this flow is always assigned the Client role
        await _userManager.AddToRoleAsync(user, Roles.Client);

        return BuildAuthResponse(user, Roles.Client);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new AuthenticationFailedException();
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new AuthenticationFailedException();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? Roles.Client;

        return BuildAuthResponse(user, primaryRole);
    }

    private AuthResponse BuildAuthResponse(ApplicationUser user, string role)
    {
        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            role
        );

        return new AuthResponse(
            token,
            expiresAtUtc,
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            role
        );
    }
}
