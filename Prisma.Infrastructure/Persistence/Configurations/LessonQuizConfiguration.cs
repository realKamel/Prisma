using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class LessonQuizConfiguration : IEntityTypeConfiguration<LessonQuiz>
{
    public void Configure(EntityTypeBuilder<LessonQuiz> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Quizzes)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.TotalDegree)
            .HasPrecision(8, 2);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}