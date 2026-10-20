using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class VideoLibraryConfiguration : IEntityTypeConfiguration<VideoLibrary>
{
    public void Configure(EntityTypeBuilder<VideoLibrary> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ProviderLibraryId).IsUnique().HasFilter("\"IsDeleted\" = false");

        builder
            .HasMany(x => x.Videos)
            .WithOne(y => y.VideoLibrary)
            .HasForeignKey(x => x.VideoLibraryId);

        builder
            .HasOne<Teacher>(x => x.Teacher)
            .WithOne(x => x.VideoLibrary)
            .HasForeignKey<VideoLibrary>(x => x.TeacherId);

        builder.Property(x => x.ProvisioningStatus).HasConversion<string>().HasMaxLength(50);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
