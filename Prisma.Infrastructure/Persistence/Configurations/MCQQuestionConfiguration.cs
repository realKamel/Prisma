using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class MCQQuestionConfiguration : IEntityTypeConfiguration<MCQQuestion>
{
    public void Configure(EntityTypeBuilder<MCQQuestion> builder)
    {
        builder
            .HasMany(q => q.Choices)
            .WithOne(c => c.Question)
            .HasForeignKey(c => c.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
