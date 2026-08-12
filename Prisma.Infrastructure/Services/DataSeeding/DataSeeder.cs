using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Infrastructure.Persistence;

namespace Prisma.Infrastructure.Services.DataSeeding;

public class DataSeeder(
    AppDbContext dbContext,
    ILogger<IDataSeeder> logger,
    RoleManager<Role> roleManager,
    UserManager<User> userManager,
    IHostEnvironment hostEnvironment,
    IConfiguration configuration)
    : IDataSeeder
{
    public async Task SeedAppDataAsync()
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            if (!hostEnvironment.IsDevelopment())
            {
                throw new Exception("There is Pending Migrations");
            }

            logger.LogInformation("Applying New Migration to Database Only (In DEV)");

            await dbContext.Database.MigrateAsync();
        }

        if (!await dbContext.Users.AnyAsync())
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Defer foreign key checking until COMMIT instead of using session_replication_role
                await dbContext.Database.ExecuteSqlRawAsync("SET CONSTRAINTS ALL DEFERRED;");

                var seedFileName = "seed_app_data.json";

                var seedPath = Path.Combine(
                    AppContext.BaseDirectory, "SeedData", seedFileName);

                logger.LogInformation("Try to Seed file : {Path} for Identity", seedPath);

                if (!File.Exists(seedPath))
                {
                    logger.LogWarning("Seed file not found: {Path}", seedPath);
                    return;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                await using var stream = File.OpenRead(seedPath);

                using var document = await JsonDocument.ParseAsync(stream,
                    new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });

                var root = document.RootElement;

                if (!await roleManager.Roles.AnyAsync())
                {
                    var roles = SeedData<Role>(root, options);

                    foreach (var role in roles)
                    {
                        await roleManager.CreateAsync(new Role(role.Name) { Id = role.Id, });
                    }
                }

                var academicYears = SeedData<AcademicYear>(root, options);

                dbContext.Set<AcademicYear>().AddRange(academicYears);

                await dbContext.SaveChangesAsync();

                var admin = new User
                {
                    FirstName = "Admin",
                    LastName = "Prisma",
                    UserName = configuration.GetSection("IdentitySeed")["AdminEmail"],
                    Email = configuration.GetSection("IdentitySeed")["AdminEmail"],
                    PhoneNumber = configuration.GetSection("IdentitySeed")["AdminPhone"],
                };

                await userManager.CreateAsync(admin,
                    configuration.GetSection("IdentitySeed")["AdminPassword"] ??
                    throw new Exception("Identity data is empty"));

                await userManager.AddToRoleAsync(admin, AppRoles.Admin);

                await dbContext.SaveChangesAsync();

                // All circular/deferred foreign keys are validated right here when the transaction commits
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occured while seeding from json file during Identity Seeding :{error}",
                    e.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        if (hostEnvironment.IsDevelopment())
        {
            await using var transaction2 = await dbContext.Database.BeginTransactionAsync();

            try
            {
                await SeedLoadTestUsersAsync();
                await transaction2.CommitAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await transaction2.RollbackAsync();
                throw;
            }
        }
    }

    private List<TEntity> SeedData<TEntity>(JsonElement root, JsonSerializerOptions options,
        JsonSerializerSettings? serializerSettings = null)
        where TEntity : class
    {
        logger.LogInformation("Seeding Check: {Path}", typeof(TEntity).Name);

        if (!root.TryGetProperty(typeof(TEntity).Name, out JsonElement output))
        {
            return [];
        }

        if (serializerSettings is null)
        {
            return JsonConvert.DeserializeObject<List<TEntity>>(output.GetRawText()) ?? [];
        }

        return JsonConvert.DeserializeObject<List<TEntity>>(output.GetRawText(), serializerSettings) ?? [];
    }

    private async Task SeedLoadTestUsersAsync(int count = 1000)
    {
        bool isSeeded = await dbContext
            .Users
            .AnyAsync(u => u.Email != null && u.Email.StartsWith("user_"));

        if (isSeeded)
        {
            return;
        }

        var passwordHasher = new PasswordHasher<User>();

        string hashedPassword = passwordHasher.HashPassword(null!, "P@ssw0rd");

        var users = new List<User>(count);

        for (int i = 1; i <= count; i++)
        {
            var email = $"user_{i}@test.com";

            users.Add(new User
            {
                Id = Guid.NewGuid(),
                FirstName = "TestUser",
                LastName = $"Num_{i}",
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false, // Satisfies PostgreSQL NOT NULL constraint
                TwoFactorEnabled = false, // Satisfies PostgreSQL NOT NULL constraint
                IsBlocked = false,
                PasswordResetConfirmed = false,
                ResetPasswordCodeAttemptCount = 0,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = true,
                AccessFailedCount = 0,
                IsOnline = false,
                PasswordHash = hashedPassword // Reuses pre-computed hash
            });
        }

        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();
    }
}