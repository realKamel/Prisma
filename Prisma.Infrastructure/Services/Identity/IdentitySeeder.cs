using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Services.Identity;

public class IdentitySeeder(ILogger<IdentitySeeder> logger, RoleManager<Role> roleManager)
    : IIdentitySeeder
{
    public async Task SeedIdentityAsync()
    {
        if (roleManager.Roles.Any())
            return;

        string[] roles =
        [
            AppClaims.Roles.Admin,
            AppClaims.Roles.Teacher,
            AppClaims.Roles.Student,
            AppClaims.Roles.Assistant
        ];

        foreach (var roleName in roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new Role(roleName));

            if (result.Succeeded)
            {
                logger.LogInformation("Created role {Role}", roleName);
            }
            else
            {
                logger.LogError("Failed to create role {Role}: {Errors}",
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}