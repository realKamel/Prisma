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

        builder.ComplexProperty(x => x.TimeDuration).ConfigureTimeDuration<Video>("TimeDuration");

        builder.Property(x => x.ProcessingStatus).HasConversion<string>().HasMaxLength(50);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
