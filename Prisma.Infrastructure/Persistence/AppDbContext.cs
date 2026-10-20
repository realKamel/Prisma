using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<
        User,
        Role,
        Guid,
        IdentityUserClaim<Guid>,
        UserRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>
    >(options),
        IDataProtectionKeyContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.Ignore<BaseDomainEvent>();

        builder.HasPostgresExtension("vector");

        // to apply all configuration for class that implements IEntityTypeConfiguration
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        // configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}
