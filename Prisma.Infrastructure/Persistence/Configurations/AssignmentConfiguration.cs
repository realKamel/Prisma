using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Infrastructure.Persistence.Configurations.ValueObjectConfigurations;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.HasKey(x => x.Id);

        // builder.HasOne(x => x.Lesson)
        //     .WithMany(x => x.Assignments)
        //     .HasForeignKey(x => x.LessonId)
        //     .OnDelete(DeleteBehavior.Cascade);

        builder.ComplexProperty(x => x.Expiry).ConfigureTimeDuration<Assignment>("Expiry");
        builder.Property(x => x.TotalMarks).HasPrecision(10, 3);
        builder.HasIndex(x => x.LessonId).IsUnique().HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
