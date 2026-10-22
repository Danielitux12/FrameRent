using System.Text;
using FrameRent.Application.Interfaces;
using FrameRent.Application.Services;
using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Identity;
using FrameRent.Infrastructure.Persistence;
using FrameRent.Infrastructure.Repositories;
using FrameRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FrameRent.Infrastructure;

/// <summary>
/// Service collection extension registering persistence, identity, security, and repository implementations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence with SQL Server
        services.AddDbContext<FrameRentDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ASP.NET Core Identity configuration
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<FrameRentDbContext>()
        .AddDefaultTokenProviders();

        // JWT Settings and Bearer Authentication
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
        var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        // HTTP Context Accessor for CurrentUserService
        services.AddHttpContextAccessor();

        // Domain & Application Repositories and Services (Scoped)
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IRentalService, RentalService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
