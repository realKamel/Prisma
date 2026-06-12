namespace Prisma.Infrastructure.Services.DataSeeding;

public interface IDataSeeder
{
    // Task SeedRolesAsync();
    // Task SeedUsersAsync();
    Task SeedAppDataAsync();
}