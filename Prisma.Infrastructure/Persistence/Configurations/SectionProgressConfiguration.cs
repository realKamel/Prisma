using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class SectionProgressConfiguration : IEntityTypeConfiguration<SectionProgress>
{
    public void Configure(EntityTypeBuilder<SectionProgress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.SectionProgresses)
            .HasForeignKey(x => x.StudentId);

        builder.HasOne(x => x.Section)
            .WithMany(x => x.Progresses)
            .HasForeignKey(x => x.SectionId);
    }
}