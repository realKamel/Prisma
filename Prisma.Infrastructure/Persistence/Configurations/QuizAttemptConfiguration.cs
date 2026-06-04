using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Degree)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.QuizAttempts)
            .HasForeignKey(x => x.StudentId);

        builder.HasOne(x => x.Quiz)
            .WithMany(x => x.Attempts)
            .HasForeignKey(x => x.QuizId);
    }
}