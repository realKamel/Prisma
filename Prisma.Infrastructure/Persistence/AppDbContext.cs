using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options)
    : IdentityDbContext<User, Role, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // to apply all configuration for class that implements IEntityTypeConfiguration
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}