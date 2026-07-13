using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Ai;
using Prisma.Domain.Interfaces;

namespace Prisma.Infrastructure.Ai;

internal sealed class AgentRagTools(
    IVectorSearchRepository vectorSearchRepository,
    IEmbeddingService embeddingService,
    IServiceScopeFactory serviceScopeFactory)

{
    [Description("Search the knowledge base for relevant lesson content to answer the user's question.")]
    public async Task<List<string>> SearchLessonsContentAsync(
        [Description("The user's question to search for.")]
        string question,
        CancellationToken ct)
    {
        var vector = await embeddingService.EmbedAsync(question, ct);

        var result = await vectorSearchRepository
            .SearchSimilarAsync(vector.ToArray(), ct);

        return result
            .GroupBy(x => x.LessonId)
            .Select(l =>
                l.Aggregate("---", (source, current) =>
                    string.Join("\n", source, current.Content)))
            .ToList();
    }

    [Description("Searches lesson transcripts for content relevant to a student's question")]
    public async Task<string> SearchLessonContentAsync(
        [Description("The student's question or search query")]
        string query,
        [Description("The lesson ID to search within")]
        int lessonId, CancellationToken cancellationToken)
    {
        var queryEmbedding = await embeddingService.EmbedAsync(query, cancellationToken);

        var chunks = await vectorSearchRepository
            .SearchSimilarAsync(queryEmbedding.ToArray(), cancellationToken, 5);

        return string.Join("\n---\n", chunks.Select(c => c.Content));
    }

    [Description("Get Student Personal Information Like Name,etc.")]
    public async Task<string> GetStudentInfo()
    {
        return await Task.FromResult("");
    }
}