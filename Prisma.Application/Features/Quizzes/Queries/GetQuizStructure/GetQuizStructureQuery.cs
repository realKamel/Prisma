
using Prisma.Application.Abstractions.Caching;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Application.Features.Quizzes.Dtos;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizStructure;

public record GetQuizStructureQuery(int QuizId) : ICacheableQuery<QuizStructureDto>
{
    // Identical for every student, so a public (not per-user) key
    public string BuildCacheKey(ICacheKeyBuilder builder) =>
        builder.ForPublic(QuizCacheKeys.Structure(QuizId));

    public IEnumerable<string> CacheTags => [QuizCacheKeys.QuizTag(QuizId)];
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);


}