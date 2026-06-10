namespace Prisma.Infrastructure.Services.IdentitySeeding;

public interface IIdentitySeeder
{
    Task SeedRolesAsync();
    Task SeedUsersAsync();
}