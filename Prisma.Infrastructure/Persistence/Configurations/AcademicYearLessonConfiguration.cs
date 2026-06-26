using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class AcademicYearLessonConfiguration : IEntityTypeConfiguration<AcademicYearLesson>
{
    public void Configure(EntityTypeBuilder<AcademicYearLesson> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => new { x.AcademicYearId, x.LessonId })
           .IsUnique()
           .HasFilter("\"IsDeleted\" = false");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
