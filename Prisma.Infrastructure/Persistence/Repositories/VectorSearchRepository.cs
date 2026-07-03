using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Persistence.Repositories;

public class VectorSearchRepository(AppDbContext dbContext)
    : Repository<LessonTranscriptChunk, Guid>(dbContext), IVectorSearchRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<LessonTranscriptChunk>> SearchSimilarAsync(float[] queryEmbedding,
        CancellationToken cancellationToken, int topK = 3)
    {
        var targetVector = new Vector(queryEmbedding);

        return await _dbContext.Set<LessonTranscriptChunk>()
            .AsNoTracking()
            .AsQueryable()
            .OrderBy(x => x.Embedding.CosineDistance(targetVector))
            .Take(topK)
            .ToListAsync(cancellationToken);
    }
}