using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class LessonTranscriptChunkConfiguration
    : IEntityTypeConfiguration<LessonTranscriptChunk>
{
    private readonly ValueComparer<float[]> floatArrayComparer = new ValueComparer<float[]>(
        (c1, c2) => c1 != null && c2 != null ? c1.AsEnumerable().SequenceEqual(c2) : c1 == c2,
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        c => c.ToArray()
    );

    public void Configure(EntityTypeBuilder<LessonTranscriptChunk> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Embedding)
            .HasColumnType("vector(1536)")
            .HasConversion<Vector>(v => new Vector(v), v => v.ToArray(), floatArrayComparer)
            .IsRequired();

        builder
            .HasIndex(x => x.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops")
            .HasStorageParameter("m", 16)
            .HasStorageParameter("ef_construction", 64);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
