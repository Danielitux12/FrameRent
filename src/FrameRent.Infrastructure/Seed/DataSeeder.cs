using FrameRent.Application.Common;
using FrameRent.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FrameRent.Infrastructure.Seed;

/// <summary>
/// Seed initialization logic ensuring default roles and the initial administrative staff user are created.
/// </summary>
public static class DataSeeder
{
    public const string StaffEmail = "staff@framerent.com";
    public const string StaffPassword = "Staff#2024!";
    public const string StaffFullName = "FrameRent Staff";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var requiredRoles = new[] { Roles.Client, Roles.Staff };
        foreach (var role in requiredRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
                }
            }
        }

        var staffUser = await userManager.FindByEmailAsync(StaffEmail);
        if (staffUser is null)
        {
            staffUser = new ApplicationUser
            {
                UserName = StaffEmail,
                Email = StaffEmail,
                FullName = StaffFullName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(staffUser, StaffPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed default staff user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(staffUser, Roles.Staff))
        {
            var roleResult = await userManager.AddToRoleAsync(staffUser, Roles.Staff);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign Staff role to default staff user: {errors}");
            }
        }
    }
}
