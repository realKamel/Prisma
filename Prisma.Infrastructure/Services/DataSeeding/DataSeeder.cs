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
    public async Task SeedAppDataAsync()
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            //throw new Exception("There is Pending Migrations");
            logger.LogInformation("Applying New Migration to Database");
            await dbContext.Database.MigrateAsync();
        }

        if (!await dbContext.Users.AnyAsync())
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // postgres ignore all FK constraints/triggers for this session
                await dbContext.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica';");

                var seedPath = Path.Combine(
                    AppContext.BaseDirectory, "SeedData", "seed_app_data.json");

                logger.LogInformation("Try to Seed file : {Path} for Identity", seedPath);

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


                if (!await roleManager.Roles.AnyAsync())
                {
                    var roles = SeedData<Role>(root, options);

                    foreach (var role in roles)
                        await roleManager.CreateAsync(new Role(role.Name) { Id = role.Id, });
                }

                if (!await userManager.Users.AnyAsync())
                {
                    List<User> users = [];
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new UserHierarchyConverter());
                    logger.LogInformation("Seeding Check: {name}", nameof(User));
                    if (root.TryGetProperty(nameof(User), out JsonElement output))
                    {
                        users = JsonConvert.DeserializeObject<List<User>>(output.GetRawText(), settings) ?? [];
                    }

                    foreach (var user in users)
                    {
                        await userManager.CreateAsync(user, "P@ssw0rd");

                        if (user is Teacher)
                        {
                            await userManager.AddToRoleAsync(user, AppRoles.Teacher);
                            if (user is Teacher teacher)
                            {
                                teacher.TeacherLandingSettings = await ReadTeacherSettingJsonFileAsync();
                            }
                        }
                        else if (user is Assistant)
                        {
                            await userManager.AddToRoleAsync(user, AppRoles.Assistant);
                        }
                        else if (user is Admin)
                        {
                            await userManager.AddToRoleAsync(user, AppRoles.Admin);
                        }
                        else
                        {
                            await userManager.AddToRoleAsync(user, AppRoles.Student);
                        }
                    }
                }

                await SeedAppDataAsync(root);

                await dbContext.SaveChangesAsync();
                // Turn constraints back on before committing
                await dbContext.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin';");

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
            UserName = "ahmed@gmail.com",
            Email = "ahmed@gmail.com"
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
        await userManager.AddToRoleAsync(teacher, AppRoles.Teacher);
    }

    public async Task SeedAppDataAsync(JsonElement root)
    {
        try
        {
            var questionSettings = new JsonSerializerSettings();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            questionSettings.Converters.Add(new QuestionConverter());

            // var root = document.RootElement;

            await SeedDataAsync<AcademicYear>(root, options);
            await SeedDataAsync<Lesson>(root, options);
            await SeedDataAsync<Section>(root, options);
            await SeedDataAsync<Assignment>(root, options);
            await SeedDataAsync<Enrollment>(root, options);
            await SeedDataAsync<Question>(root, options, questionSettings);
            await SeedDataAsync<Quiz>(root, options);
            await SeedDataAsync<Choice>(root, options);
            await SeedDataAsync<QuestionLessonQuiz>(root, options);
            await SeedDataAsync<QuizAttempt>(root, options);
            await SeedDataAsync<AttemptAnswer>(root, options);
            await SeedDataAsync<Report>(root, options);
            await SeedDataAsync<SectionProgress>(root, options);
            await SeedDataAsync<AssignmentSubmission>(root, options);
            await SeedDataAsync<RedeemCode>(root, options);
            await SeedDataAsync<GeneratedCode>(root, options);
            await SeedDataAsync<Payment>(root, options);
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

    private async Task SeedDataAsync<TEntity>(JsonElement root, JsonSerializerOptions options,
        JsonSerializerSettings? serializerSettings = null)
        where TEntity : class
    {
        logger.LogInformation("Seeding Check: {Path}", typeof(TEntity).Name);

        if (root.TryGetProperty(typeof(TEntity).Name, out JsonElement output) &&
            !await dbContext.Set<TEntity>().AnyAsync())
        {
            List<TEntity> entities;
            if (serializerSettings is null)
            {
                entities = JsonConvert
                    .DeserializeObject<List<TEntity>>(output.GetRawText()) ?? [];
            }
            else
            {
                entities = JsonConvert
                    .DeserializeObject<List<TEntity>>(output.GetRawText(), serializerSettings) ?? [];
            }

            dbContext.Set<TEntity>().AddRange(entities);
        }
    }
}