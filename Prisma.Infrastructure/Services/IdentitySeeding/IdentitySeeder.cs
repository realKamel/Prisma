using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Infrastructure.Persistence;

namespace Prisma.Infrastructure.Services.IdentitySeeding;

public class IdentitySeeder(
    AppDbContext dbContext,
    ILogger<IdentitySeeder> logger,
    RoleManager<Role> roleManager,
    UserManager<User> userManager)
    : IIdentitySeeder
{
    public async Task SeedRolesAsync()
    {
        var migrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (migrations.Any())
        {
            throw new Exception("There is Pending Migrations");
        }

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

    public async Task SeedUsersAsync()
    {
        var migrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (migrations.Any())
        {
            throw new Exception("There is Pending Migrations");
        }

        if (await userManager.Users.OfType<Teacher>().AnyAsync())
        {
            return;
        }

        var teacher = new Teacher()
        {
            Id = Guid.CreateVersion7(),
            FirstName = "Ahmed",
            LastName = "Mostafa",
            Subject = "English",
            PhoneNumber = "01010101010",
            UserName = "ahmed@prisma.com",
            Email = "ahmed@prisma.com"
        };

        await userManager.CreateAsync(teacher, "AhmedP@ssword");
    }
}