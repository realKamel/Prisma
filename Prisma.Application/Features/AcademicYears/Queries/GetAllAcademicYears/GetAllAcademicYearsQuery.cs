using Ardalis.Result;
using Prisma.Application.Common.Interfaces;
using Prisma.Application.Features.AcademicYears.Dtos;

namespace Prisma.Application.Features.AcademicYears.Queries.GetAllAcademicYears;

public record GetAllAcademicYearsQuery : ICacheableQuery<Result<List<AcademicYearOptionDto>>>
{
    //TODO: Must be evicted if updated immediately
    public string BuildCacheKey(ICacheKeyBuilder builder)
    {
        return builder?.ForPublic("academic-years")
            ?? throw new ArgumentNullException(nameof(builder));
    }

    public TimeSpan? Expiration { get; } = TimeSpan.FromDays(1);
}
