using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.HasKey(x => x.Id);

        // builder.HasOne(x => x.Lesson)
        //     .WithMany(x => x.Assignments)
        //     .HasForeignKey(x => x.LessonId)
        //     .OnDelete(DeleteBehavior.Cascade);

        builder
        .HasIndex(x => x.LessonId)
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}