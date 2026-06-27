using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");


        builder
        .HasMany(x => x.Claims)
        .WithOne()
        .HasForeignKey(x => x.UserId);


        builder
        .HasIndex(u => u.NormalizedEmail)
        .HasDatabaseName("EmailIndex");

        builder
        .HasIndex("NormalizedUserName")
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false")
        .HasDatabaseName("UserNameIndex");

        builder.HasIndex(x => x.PhoneNumber)
       .IsUnique()
       .HasFilter("\"IsDeleted\" = false");
    }
}