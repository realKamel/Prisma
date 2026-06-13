using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.Students)
            .WithOne(x => x.AcademicYear)
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Teachers)
            .WithMany(x => x.AcademicYears);

        builder.HasMany(x => x.Lessons)
            .WithMany(x => x.AcademicYears);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}