using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Teachers.Queries.DTOs;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboard.GetTeacherDashboardData;

public sealed record GetTeacherDashboardQuery() : IRequest<Result<GetTeacherDashboardResponse>>;

public sealed record GetTeacherDashboardResponse(
    Stats Stats,
    BestSales[] BestSales,
    AuditLogDto[] Logs,
    RecentSale[] RecentSales,
    Attention Attention
);

public sealed record Stats(
    int TotalActiveStudents,
    int TotalActiveLessons,
    decimal TotalCompletedLessonsAgainstThisMonth,
    decimal? TotalCompletedLessonsAgainstLastMonthPercentage,
    decimal OverallCompletionRate
);

public sealed record BestSales(
    int LessonId,
    decimal Amount,
    int StudentCount,
    string? LessonTitle,
    decimal CompletionRate
);

public sealed record RecentSale(
    int LessonId,
    string? LessonTitle,
    decimal Amount,
    DateTimeOffset CreatedAt
);

public sealed record Attention(
    int AssignmentsToGrade,
    int QuizzesToGrade,
    int UnreadMessages,
    int LessonsWithoutEnrollments,
    int DraftLessons
);
