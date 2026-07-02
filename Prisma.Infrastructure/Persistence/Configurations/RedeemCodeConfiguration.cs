using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class RedeemCodeConfiguration : IEntityTypeConfiguration<RedeemCode>
{
    public void Configure(EntityTypeBuilder<RedeemCode> builder)
    {
        builder.Property(x => x.Prefix)
            .HasMaxLength(10);

        builder.HasOne(x => x.Lesson)
            .WithMany(l => l.RedeemCodes)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcademicYear)
            .WithMany()
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByTeacher)
            .WithMany()
            .HasForeignKey(x => x.CreatedByTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.GeneratedCodes)
            .WithOne(x => x.Batch)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}