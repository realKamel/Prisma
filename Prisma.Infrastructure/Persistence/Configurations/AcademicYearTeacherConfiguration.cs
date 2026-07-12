using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal class AcademicYearTeacherConfiguration : IEntityTypeConfiguration<AcademicYearTeacher>
{
    public void Configure(EntityTypeBuilder<AcademicYearTeacher> builder)
    {
        builder.HasKey(e => e.Id);


        builder.HasIndex(x => new { x.AcademicYearId, x.TeacherId })
               .IsUnique()
               .HasFilter("\"IsDeleted\" = false");

        builder.HasOne(x => x.Teacher)
               .WithMany(a => a.AcademicYears)
               .HasForeignKey(s => s.TeacherId);

        builder.HasOne(x => x.AcademicYear)
               .WithMany(y => y.Teachers)
               .HasForeignKey(x => x.AcademicYearId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
