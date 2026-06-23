using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;

public record GetTeacherDashboardStatusQuery() : IRequest<Result<GetTeacherDashboardStatusResponse>>;

public record GetTeacherDashboardStatusResponse(
    Stats Stats,
    WeekEarnings WeekEarnings,
    BestSales[] BestSales,
    string[] Logs
);

public record Stats(
    decimal TotalEarningsForThisMonth,
    decimal TotalEarningsAgainstLastMonth,
    int TotalActiveStudents,
    int TotalActiveLessons,
    decimal TotalCompletedLessonsAgainstThisMonth,
    decimal TotalCompletedLessonsAgainstLastMonthPercentage);

public record WeekEarnings(
    decimal TotalEarningsForThisWeek,
    object[] Data
);

public record BestSales(int LessonId, decimal Amount, int StudentCount);