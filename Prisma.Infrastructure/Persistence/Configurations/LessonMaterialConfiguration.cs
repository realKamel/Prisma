using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class LessonMaterialConfiguration : IEntityTypeConfiguration<LessonMaterial>
{
    public void Configure(EntityTypeBuilder<LessonMaterial> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
