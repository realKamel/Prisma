namespace Prisma.Infrastructure.Services.Identity;

public interface IIdentitySeeder
{
    Task SeedIdentityAsync();
}