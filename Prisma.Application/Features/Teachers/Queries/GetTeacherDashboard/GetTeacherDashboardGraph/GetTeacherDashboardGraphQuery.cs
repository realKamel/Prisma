using Ardalis.Result;
using Prisma.Application.Abstractions.Caching;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboard.GetTeacherDashboardGraph;

// TeacherId is set by the controller from the authenticated user (never from client input).
// It is part of the cache key so every teacher gets their own cache entry.
public sealed record GetTeacherDashboardGraphQuery()
    : ICacheableQuery<Result<GetTeacherDashboardGraphResponse>>
{
    public string BuildCacheKey(ICacheKeyBuilder builder)
    {
        return builder?.ForUser("teacher_dashboard-graph")
            ?? throw new ArgumentNullException(nameof(builder));
    }

    public TimeSpan? Expiration { get; } = TimeSpan.FromHours(1);
}

public sealed record GetTeacherDashboardGraphResponse(GraphStats Stats, Earnings Earnings);

public sealed record GraphStats(
    decimal TotalEarningsForThisMonth,
    decimal? TotalEarningsAgainstLastMonth
);

// 60 zero-filled days, oldest -> newest, bucketed by Cairo calendar day.
// The UI slices this into 7 / 30 day ranges and compares each against the range before it.
public sealed record Earnings(EarningEntry[] Daily);

public sealed record EarningEntry(string Date, decimal Earning, int Enrollments);
