using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Note).HasMaxLength(Bookmark.MaxNoteLength);

        builder
            .HasOne(x => x.Student)
            .WithMany(x => x.Bookmarks)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Section)
            .WithMany(x => x.Bookmarks)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PublicId).IsUnique().HasFilter("\"IsDeleted\" = false");

        // One bookmark per student, section and second. Also serves the list query.
        builder
            .HasIndex(x => new { x.StudentId, x.SectionId, x.PositionSeconds })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}