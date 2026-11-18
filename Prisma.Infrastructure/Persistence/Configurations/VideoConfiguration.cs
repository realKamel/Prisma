using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Infrastructure.Persistence.Configurations.ValueObjectConfigurations;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ProviderVideoId).IsUnique().HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(x => new { x.ProviderVideoLibraryId, x.ProviderVideoId }).IsUnique();

        builder.ComplexProperty(x => x.TimeDuration).ConfigureTimeDuration<Video>("TimeDuration");

        builder
            .Property(v => v.Status)
            .HasConversion(status => status.Value, value => VideoStatus.FromValue(value))
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
