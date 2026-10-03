using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options), IDataProtectionKeyContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // to apply all configuration for class that implements IEntityTypeConfiguration
        base.OnModelCreating(builder);

        builder.HasPostgresExtension("vector");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}