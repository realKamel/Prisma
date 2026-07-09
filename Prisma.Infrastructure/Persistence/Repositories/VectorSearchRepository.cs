using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Persistence.Repositories;

public class VectorSearchRepository(IServiceProvider sp) : IVectorSearchRepository
{
    public async Task<IReadOnlyList<LessonTranscriptChunk>> SearchSimilarAsync(float[] queryEmbedding,
        CancellationToken cancellationToken, int topK = 3, int? lessonId = null)
    {
        var targetVector = new Vector(queryEmbedding);
        await using var scope = sp.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // await using var dbContext = await dbFactory.CreateDbContextAsync(cancellationToken);

        var query = db
            .Set<LessonTranscriptChunk>()
            .AsNoTracking();

        if (lessonId is not null)
        {
            query = query.Where(x => x.LessonId == lessonId.Value);
        }

        // pgvector cosine similarity query via EF Core / raw SQL, HNSW index
        return await query
            .OrderBy(x => x.Embedding.CosineDistance(targetVector))
            .Take(topK)
            .ToListAsync(cancellationToken);
    }
}