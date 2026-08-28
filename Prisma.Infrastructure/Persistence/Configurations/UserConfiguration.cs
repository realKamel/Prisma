using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.HasMany(x => x.Claims).WithOne().HasForeignKey(x => x.UserId);

        builder
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("EmailIndex")
            .HasFilter("\"IsDeleted\" = false");

        builder
            .HasIndex(u => u.NormalizedUserName)
            .IsUnique()
            .HasDatabaseName("UserNameIndex")
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(x => x.PhoneNumber).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}
