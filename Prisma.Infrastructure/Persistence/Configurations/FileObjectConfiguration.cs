using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.ObjectLibraryAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class FileObjectConfiguration : IEntityTypeConfiguration<FileObject>
{
    public void Configure(EntityTypeBuilder<FileObject> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
