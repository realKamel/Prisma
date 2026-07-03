using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Interfaces;

public interface IVectorSearchRepository : IRepository<LessonTranscriptChunk, Guid>
{
    Task<IReadOnlyList<LessonTranscriptChunk>> SearchSimilarAsync(float[] queryEmbedding,
        CancellationToken cancellationToken, int topK = 3);
}