using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasDiscriminator<string>("QuestionType")
            .HasValue<MCQQuestion>("MCQ")
            .HasValue<WrittenQuestion>("Written");

        //builder.Property(q => q.Type)
        //  .HasConversion<string>()
        //  .HasMaxLength(20)
        //  .IsRequired();

        builder.HasMany(q => q.AttemptAnswers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(q => q.QuestionLessons)
            .WithOne(ql => ql.Question)
            .HasForeignKey(ql => ql.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}