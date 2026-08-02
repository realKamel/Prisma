
using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Admin.Queries.GetAdminStatsQuery;

public record GetAdminStatsQuery() : IRequest<Result<AdminStatsResponseDto>>;

public record AdminStatsResponseDto(
    DateTimeOffset CurrentDateTime,
    List<KpiDto> Kpis,
    decimal WeeklyTotal,
    List<RevenueWeekDto> RevenueWeek
);

public record KpiDto(
    string Id,
    decimal Value,
    decimal Delta
);

public record RevenueWeekDto(
    DateTimeOffset Date,
    decimal Amount,
    bool IsToday
);