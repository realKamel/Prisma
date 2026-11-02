using Ardalis.Result;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboard.GetTeacherDashboardGraph;

// TeacherId is set by the controller from the authenticated user (never from client input).
// It is part of the cache key so every teacher gets their own cache entry.
public record GetTeacherDashboardGraphQuery(Guid TeacherId)
    : ICacheableQuery<Result<GetTeacherDashboardGraphResponse>>
{
    public string CacheKey => $"TeacherDashboardGraph:{TeacherId}";
    public TimeSpan? Expiration { get; } = TimeSpan.FromHours(1);
}

public record GetTeacherDashboardGraphResponse(
    GraphStats Stats,
    Earnings Earnings
);

public record GraphStats(decimal TotalEarningsForThisMonth,
    decimal? TotalEarningsAgainstLastMonth);

// 60 zero-filled days, oldest -> newest, bucketed by Cairo calendar day.
// The UI slices this into 7 / 30 day ranges and compares each against the range before it.
public record Earnings(EarningEntry[] Daily);

public record EarningEntry(string Date, decimal Earning, int Enrollments);