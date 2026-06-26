using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasIndex(x => x.PhoneNumber).IsUnique();

        builder
        .HasMany(x => x.Claims)
        .WithOne()
        .HasForeignKey(x => x.UserId);
    }
}