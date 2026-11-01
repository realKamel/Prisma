using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Teachers.Queries.DTOs;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;

public sealed record GetTeacherDashboardStatusQuery(Guid? TeacherId = null)
    : IRequest<Result<GetTeacherDashboardStatusResponse>>;

public sealed record GetTeacherDashboardStatusResponse(
    Stats Stats,
    WeekEarnings WeekEarnings,
    BestSales[] BestSales,
    AuditLogDto[] Logs
);

public sealed record Stats(
    decimal TotalEarningsForThisMonth,
    decimal TotalEarningsAgainstLastMonth,
    int TotalActiveStudents,
    int TotalActiveLessons,
    decimal TotalCompletedLessonsAgainstThisMonth,
    decimal TotalCompletedLessonsAgainstLastMonthPercentage
);

public sealed record WeekEarnings(decimal TotalEarningsForThisWeek, EarningEntry[] Data);

public sealed record EarningEntry(string Day, decimal Earning);

public sealed record BestSales(int LessonId, decimal Amount, int StudentCount);
