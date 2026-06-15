using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .HasPrecision(12, 2);

        builder.HasMany(l => l.RedeemCodes)
            .WithOne(r => r.Lesson)
            .HasForeignKey(r => r.LessonId);

        builder.HasMany(x => x.LessonMaterials)
         .WithOne(x => x.Lesson)
         .HasForeignKey(x => x.LessonId)
         .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}