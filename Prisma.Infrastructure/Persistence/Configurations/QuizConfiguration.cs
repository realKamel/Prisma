using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.HasKey(x => x.Id);

        // builder.HasOne(x => x.Lesson)
        //     .WithMany(x => x.Quizzes)
        //     .HasForeignKey(x => x.LessonId)
        //     .OnDelete(DeleteBehavior.Cascade);

        builder.Property(q => q.Scope)
            .HasConversion<string>()
            .HasMaxLength(25)
            .IsRequired();

        builder.HasOne(q => q.AcademicYear)
            .WithMany(s => s.Quizzes)
            .HasForeignKey(q => q.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Property(x => x.TotalDegree)
            .HasPrecision(8, 2);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}