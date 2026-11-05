using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.ObjectLibraryAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class ObjectStorageConfiguration : IEntityTypeConfiguration<ObjectStorage>
{
    public void Configure(EntityTypeBuilder<ObjectStorage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TeacherId).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}
