using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class ExtractionJobConfiguration : IEntityTypeConfiguration<ExtractionJob>
{
    public void Configure(EntityTypeBuilder<ExtractionJob> builder)
    {
        builder.ToTable("ExtractionJobs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        // PostgreSQL uses 'text' not 'nvarchar'
        builder.Property(e => e.QuestionsJson)
            .HasColumnType("text")
            .HasDefaultValue("[]");
    }
}
