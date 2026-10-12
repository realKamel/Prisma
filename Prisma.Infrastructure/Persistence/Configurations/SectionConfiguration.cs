using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Infrastructure.Persistence.Configurations.ValueObjectConfigurations;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.HasKey(x => x.Id);

        builder.ComplexProperty(x => x.TimeDuration).ConfigureTimeDuration<Section>("TimeDuration");
        builder
            .HasOne(x => x.Lesson)
            .WithMany(x => x.Sections)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PublicId).IsUnique().HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
