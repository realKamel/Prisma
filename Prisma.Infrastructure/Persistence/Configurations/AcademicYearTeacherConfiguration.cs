using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal class AcademicYearTeacherConfiguration : IEntityTypeConfiguration<AcademicYearTeacher>
{
    public void Configure(EntityTypeBuilder<AcademicYearTeacher> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => new { x.AcademicYearId, x.TeacherId })
           .IsUnique()
           .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
