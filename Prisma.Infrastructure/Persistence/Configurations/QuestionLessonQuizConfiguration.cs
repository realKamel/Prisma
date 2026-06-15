using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class QuestionLessonQuizConfiguration : IEntityTypeConfiguration<QuestionLessonQuiz>
{
    public void Configure(EntityTypeBuilder<QuestionLessonQuiz> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Degree)
            .HasPrecision(8, 2);

        builder.HasOne(x => x.Quiz)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.LessonQuizId);

        builder.HasOne(x => x.Question)
            .WithMany(x => x.QuestionLessons)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // to not repeat the same question in the same quiz
        builder.HasIndex(x => new { x.LessonQuizId, x.QuestionId }).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}