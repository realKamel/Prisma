using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Degree)
            .HasPrecision(8, 2);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(x => x.Student)
            .WithMany(x => x.QuizAttempts)
            .HasForeignKey(x => x.StudentId);

        builder.HasOne(x => x.Quiz)
            .WithMany(x => x.Attempts)
            .HasForeignKey(x => x.QuizId);

        // each student has one try to attemp the quiz
        builder.HasIndex(a => new { a.QuizId, a.StudentId });

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}