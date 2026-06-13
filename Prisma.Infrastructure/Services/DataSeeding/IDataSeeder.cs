namespace Prisma.Infrastructure.Services.DataSeeding;

public interface IDataSeeder
{
    // Task SeedRolesAsync();
    // Task SeedIdentityAsync();
    Task SeedAppDataAsync();
}