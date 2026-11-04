using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Teachers.Queries.DTOs;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboard.GetTeacherDashboardData;

public record GetTeacherDashboardQuery()
    : IRequest<Result<GetTeacherDashboardResponse>>;

public record GetTeacherDashboardResponse(
    Stats Stats,
    BestSales[] BestSales,
    AuditLogDto[] Logs,
    RecentSale[] RecentSales,
    Attention Attention
);

public record Stats(
    int TotalActiveStudents,
    int TotalActiveLessons,
    decimal TotalCompletedLessonsAgainstThisMonth,
    decimal? TotalCompletedLessonsAgainstLastMonthPercentage,
    decimal OverallCompletionRate); 

public record BestSales(
    int LessonId,
    decimal Amount,
    int StudentCount,
    string? LessonTitle,
    decimal CompletionRate);

public record RecentSale(int LessonId, string? LessonTitle, decimal Amount, DateTimeOffset CreatedAt);

public record Attention(
    int AssignmentsToGrade,
    int QuizzesToGrade,
    int UnreadMessages,
    int LessonsWithoutEnrollments,
    int DraftLessons);