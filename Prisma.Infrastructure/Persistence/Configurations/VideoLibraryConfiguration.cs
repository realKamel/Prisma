using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class VideoLibraryConfiguration : IEntityTypeConfiguration<VideoLibrary>
{
    public void Configure(EntityTypeBuilder<VideoLibrary> builder)
    {
        builder.HasKey(x => x.Id);

        // builder.HasIndex(x => x.ProviderLibraryId).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder
            .HasIndex(x => x.ProviderLibraryId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ProviderLibraryId\" IS NOT NULL");

        builder
            .HasMany(x => x.Videos)
            .WithOne(y => y.VideoLibrary)
            .HasForeignKey(x => x.VideoLibraryId);

        // builder
        //     .HasOne<Teacher>(x => x.Teacher)
        //     .WithOne(x => x.VideoLibrary)
        //     .HasForeignKey<VideoLibrary>(x => x.TeacherId);
        //

        builder.HasIndex(x => x.TeacherId).IsUnique().HasFilter("\"IsDeleted\" = false");

        builder
            .Property(x => x.Status)
            .HasConversion(status => status.Value, value => VideoLibraryStatus.FromValue(value));

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
