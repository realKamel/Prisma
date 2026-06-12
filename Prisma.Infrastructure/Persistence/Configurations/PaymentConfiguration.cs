using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Amount)
            .HasPrecision(12, 2);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}