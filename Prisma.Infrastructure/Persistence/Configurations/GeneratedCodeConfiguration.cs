using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class GeneratedCodeConfiguration : IEntityTypeConfiguration<GeneratedCode>
{
    public void Configure(EntityTypeBuilder<GeneratedCode> builder)
    {
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        // Unique per batch only (not globally).
        builder.HasIndex(x => new { x.BatchId, x.Code })
            .IsUnique();

        builder.HasOne(x => x.RedeemedByStudent)
            .WithMany()
            .HasForeignKey(x => x.RedeemedByStudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inverse side (Enrollment -> GeneratedCode) is configured in EnrollmentConfiguration.

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}