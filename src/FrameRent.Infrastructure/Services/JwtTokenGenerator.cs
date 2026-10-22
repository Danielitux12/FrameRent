using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FrameRent.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FrameRent.Infrastructure.Services;

/// <summary>
/// Generates signed JWT tokens with claims required for authentication and authorization.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IOptions<JwtSettings> _jwtOptions;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(string userId, string email, string fullName, string role)
    {
        var jwtSettings = _jwtOptions.Value;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(ClaimTypes.NameIdentifier, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new("fullName", fullName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAtUtc,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(securityToken);

        return (tokenString, expiresAtUtc);
    }
}
