using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class LessonMaterialConfiguration : IEntityTypeConfiguration<LessonMaterial>
{
    public void Configure(EntityTypeBuilder<LessonMaterial> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}