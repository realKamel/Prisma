using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.AdminDashboard.Queries.GetAdminStats;

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