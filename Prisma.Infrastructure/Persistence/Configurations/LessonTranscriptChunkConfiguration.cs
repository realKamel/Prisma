using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class LessonTranscriptChunkConfiguration : IEntityTypeConfiguration<LessonTranscriptChunk>
{
    public void Configure(EntityTypeBuilder<LessonTranscriptChunk> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Embedding)
            .HasColumnType("vector(1536)")
            .HasConversion<Vector>(v => new Vector(v), v => v.ToArray())
            .IsRequired();

        builder.HasIndex(x => x.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops")
            .HasStorageParameter("m", 16)
            .HasStorageParameter("ef_construction", 64);


        // builder.HasQueryFilter(x => !x.IsDeleted);
    }
}