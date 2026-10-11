using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.ActivityLogs.Queries.GetAdminStatsQuery;

public record GetAdminStatsQuery() : IRequest<Result<AdminStatsResponseDto>>;

public record AdminStatsResponseDto(
    DateTimeOffset CurrentDateTime,
    List<KpiDto> Kpis,
    decimal WeeklyTotal,
    IList<RevenueWeekDto> RevenueWeek
);

public record KpiDto(string Id, decimal Value, decimal Delta);

public record RevenueWeekDto(DateTimeOffset Date, decimal Amount, bool IsToday);
