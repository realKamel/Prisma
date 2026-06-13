using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Infrastructure.Persistence;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Prisma.Infrastructure.Services.DataSeeding;

public class DataSeeder(
    AppDbContext dbContext,
    ILogger<IDataSeeder> logger,
    RoleManager<Role> roleManager,
    UserManager<User> userManager)
    : IDataSeeder
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

    public async Task SeedTeacherSettingAsync()
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

        try
        {
            // teacher.TeacherLandingSettings = await ReadJsonFileAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while seeding from json file");
            throw;
        }

        await userManager.CreateAsync(teacher, "AhmedP@ssword");
        await userManager.AddToRoleAsync(teacher, AppClaims.Roles.Teacher);
    }

    public async Task SeedAppDataAsync()
    {
        var migrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (migrations.Any())
        {
            throw new Exception("There is Pending Migrations");
        }

        try
        {
            var seedPath = Path.Combine(
                AppContext.BaseDirectory, "SeedData", "seed_app_data.json");

            Console.WriteLine(seedPath);

            if (!File.Exists(seedPath))
            {
                logger.LogWarning("Seed file not found: {Path}", seedPath);
                return;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            await using FileStream stream = File.OpenRead(seedPath);
            using JsonDocument document = await JsonDocument.ParseAsync(stream,
                new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });

            var root = document.RootElement;

            await SeedData<AcademicYear>(root, options);
            await SeedData<Lesson>(root, options);
            await SeedData<Section>(root, options);
            await SeedQuestionData(root);
            await SeedUsersData(root);
            await SeedData<Role>(root, options);
            await SeedData<Choice>(root, options);
            await SeedData<LessonQuiz>(root, options);
            await SeedData<QuestionLessonQuiz>(root, options);
            await SeedData<QuizAttempt>(root, options);
            await SeedData<AttemptAnswer>(root, options);
            await SeedData<Report>(root, options);
            await SeedData<SectionProgress>(root, options);
            await SeedData<Assignment>(root, options);
            await SeedData<AssignmentSubmission>(root, options);
            await SeedData<RedeemCode>(root, options);
            await SeedData<Enrollment>(root, options);
            await SeedData<Payment>(root, options);
            // await SeedData<AspNetUserRole>()
            dbContext.Users.OfType<Teacher>().FirstOrDefault()?.TeacherLandingSettings =
                await ReadTeacherSettingJsonFileAsync();
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError("An error occured while seeding from json file {e}", e.Message);
            throw;
        }
    }

    private async Task<TeacherLandingSettings?> ReadTeacherSettingJsonFileAsync(CancellationToken ct = default)
    {
        var seedPath = Path.Combine(
            AppContext.BaseDirectory, "SeedData", "TeacherSettingDataSeed.json");

        Console.WriteLine(seedPath);

        if (!File.Exists(seedPath))
        {
            logger.LogWarning("Seed file not found: {Path}", seedPath);
            return null;
        }

        await using var stream = File.OpenRead(seedPath);

        var entities = JsonSerializer.Deserialize<TeacherLandingSettings>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return entities;
    }

    private async Task SeedData<TEntity>(JsonElement root, JsonSerializerOptions options) where TEntity : class
    {
        logger.LogInformation("Seeding Check: {Path}", typeof(TEntity).Name);
        if (root.TryGetProperty(typeof(TEntity).Name, out JsonElement output) &&
            !await dbContext.Set<TEntity>().AnyAsync())
        {
            var academicYears =
                JsonConvert.DeserializeObject<List<TEntity>>(output.GetRawText());
            dbContext.Set<TEntity>().AddRange(academicYears ?? []);
        }
    }

    private async Task SeedQuestionData(JsonElement root)
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new QuestionConverter());
        logger.LogInformation("Seeding Check: {name}", nameof(Question));
        if (root.TryGetProperty(nameof(Question), out JsonElement output) &&
            !await dbContext.Set<Question>().AnyAsync())
        {
            var academicYears =
                JsonConvert.DeserializeObject<List<Question>>(output.GetRawText(), settings);
            dbContext.Set<Question>().AddRange(academicYears ?? []);
        }
    }

    private async Task SeedUsersData(JsonElement root)
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new UserHierarchyConverter());
        logger.LogInformation("Seeding Check: {name}", nameof(User));
        if (root.TryGetProperty(nameof(User), out JsonElement output) &&
            !await dbContext.Set<User>().AnyAsync())
        {
            var academicYears =
                JsonConvert.DeserializeObject<List<User>>(output.GetRawText(), settings);
            dbContext.Set<User>().AddRange(academicYears ?? []);
            // await dbContext.SaveChangesAsync();
        }
    }
}