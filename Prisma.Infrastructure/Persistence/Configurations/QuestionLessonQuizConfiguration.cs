using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class QuestionLessonQuizConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Degree).HasPrecision(8, 2);

        builder
            .HasOne(x => x.Quiz)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Question)
            .WithMany(x => x.QuizQuestions)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // to not repeat the same question in the same quiz
        builder
            .HasIndex(x => new { x.QuizId, x.QuestionId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
