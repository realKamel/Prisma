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
    decimal Delta // الرقم الصافي (مثلاً: 23 أو -5) والفرونت يحدد اتجاه السهم واللون
);

public record RevenueWeekDto(
    DateTimeOffset Date, // تاريخ اليوم الصافي والفرونت يجيب منه اسم اليوم بالعربي (أحد، اثنين...)
    decimal Amount,
    bool IsToday
);