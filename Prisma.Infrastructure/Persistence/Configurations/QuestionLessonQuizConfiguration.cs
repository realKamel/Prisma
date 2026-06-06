using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class QuestionLessonQuizConfiguration : IEntityTypeConfiguration<QuestionLessonQuiz>
{
    public void Configure(EntityTypeBuilder<QuestionLessonQuiz> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Degree)
            .HasPrecision(8, 2);

        builder.HasOne(x => x.LessonQuiz)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.LessonQuizId);

        builder.HasOne(x => x.Question)
            .WithMany(x => x.QuestionLessons)
            .HasForeignKey(x => x.QuestionId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}