using System.Linq.Expressions;
using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Teachers.Queries.DTOs;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using Prisma.Domain.Specifications.Chat;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboard.GetTeacherDashboardData;

public class GetTeacherDashboardQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<GetTeacherDashboardQuery, Result<GetTeacherDashboardResponse>>
{
    private const int PeriodDays = 30;
    private const int WindowDays = PeriodDays * 2;

    public async Task<Result<GetTeacherDashboardResponse>> Handle(
        GetTeacherDashboardQuery request,
        CancellationToken cancellationToken
    )
    {
        var now = DateTimeOffset.UtcNow;
        var today = ToDate(now);

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var auditRepo = unitOfWork.GetOrCreateRepository<AuditLog, long>();

        var teacherId = currentUserService.UserId;
        string? teacherEmail = currentUserService.Email;

        if (teacherId == null || teacherEmail == null)
        {
            return Result.Error("Current user is not authenticated.");
        }

        var logsSpec = new PagedLogsOrderByCreatedAtSpec<AuditLogDto>(
            l => l.UserEmail != SystemConstants.SystemEmail && (l.UserEmail == teacherEmail),
            l => new AuditLogDto(l.Id, l.UserEmail, l.Action, l.TableName, l.CreatedAt),
            0,
            10
        );

        var activeStudentsCount = await studentRepo.CountAsync(
            new ActiveStudentSpecification(teacherId),
            cancellationToken
        );

        var activeLessonsCount = await lessonRepo.CountAsync(
            new ActiveLessonsSpecification(teacherId),
            cancellationToken
        );

        var draftLessonsCount = await lessonRepo.CountAsync(
            new DraftLessonsSpecification(teacherId),
            cancellationToken
        );

        var (assignmentsToGrade, quizzesToGrade, unreadMessages) = await CountPendingWorkAsync(
            teacherId,
            cancellationToken
        );

        var logs = (await auditRepo.ListAsync(logsSpec, cancellationToken)).ToArray();

        var queryFrom = now.AddDays(-(WindowDays + 1));

        var windowEnrollments = await enrollmentRepo.ListAsync(
            new EnrollmentWithProjectionSpec<EnrollmentInfo>(
                e => e.CreatedAt >= queryFrom && teacherId == e.Lesson!.TeacherId,
                e => new EnrollmentInfo(
                    e.CreatedAt,
                    e.Payment != null ? e.Payment.Money.Amount : 0,
                    e.IsCompleted,
                    e.CompletedAt,
                    e.Lesson!.Id
                )
            ),
            cancellationToken
        );

        // ---- Completions ------------------------------------------------------------------------
        var periodStart = today.AddDays(-(PeriodDays - 1));
        var previousPeriodStart = today.AddDays(-(PeriodDays * 2 - 1));

        var completionDates = windowEnrollments
            .Where(e => e.IsCompleted && e.CompletedAt.HasValue)
            .Select(e => ToDate(e.CompletedAt!.Value))
            .ToArray();

        var completedThisMonth = completionDates.Count(d => d >= periodStart && d <= today);
        var completedLastMonth = completionDates.Count(d =>
            d >= previousPeriodStart && d < periodStart
        );

        // ---- Overall completion rate: two database counts ----------------------------------------
        var totalEnrollments = await enrollmentRepo.CountAsync(
            new EnrollmentsOfActiveLessonsSpecification(teacherId),
            cancellationToken
        );
        var completedEnrollments = await enrollmentRepo.CountAsync(
            new EnrollmentsOfActiveLessonsSpecification(teacherId, completedOnly: true),
            cancellationToken
        );

        var overallCompletionRate =
            totalEnrollments == 0
                ? 0
                : Math.Round((decimal)completedEnrollments / totalEnrollments * 100, 0);

        // ---- Active lessons nobody enrolled in during the last 30 days ---------------------------
        var lessonsWithoutEnrollments = await lessonRepo.CountAsync(
            new LessonsWithoutRecentEnrollmentsSpecification(teacherId, now.AddDays(-PeriodDays)),
            cancellationToken
        );

        // ---- Best selling lessons: the database ranks and counts, only 5 rows come back ---------
        var topSales = await lessonRepo.ListAsync(
            new TopSellingLessonsSpecification(teacherId, 5),
            cancellationToken
        );

        var bestSales = topSales
            .Select(x => new BestSales(
                x.Id,
                x.Amount,
                x.EnrollmentCount,
                x.Title,
                Math.Round((decimal)x.CompletedCount / x.EnrollmentCount * 100, 0)
            ))
            .ToArray();

        var recent = windowEnrollments
            .Where(e => e.CreatedAt.HasValue)
            .OrderByDescending(e => e.CreatedAt)
            .Take(8)
            .ToArray();

        // ---- Titles for the recent sales: one small query, only for those lessons ----------------
        var recentLessonIds = recent.Select(e => e.LessonId).Distinct().ToArray();

        var lessonInfo =
            recentLessonIds.Length == 0
                ? new Dictionary<int, LessonDisplayInfo>()
                : (
                    await lessonRepo.ListAsync(
                        new LessonsByIdsWithProjectionSpec<LessonDisplayInfo>(
                            recentLessonIds,
                            l => new LessonDisplayInfo(l.Id, l.Title)
                        ),
                        cancellationToken
                    )
                ).ToDictionary(l => l.Id);

        // ---- Recent sales -----------------------------------------------------------------------
        var recentSales = recent
            .Select(e => new RecentSale(
                e.LessonId,
                lessonInfo.GetValueOrDefault(e.LessonId)?.Title,
                e.PaymentAmount,
                e.CreatedAt!.Value
            ))
            .ToArray();

        return new GetTeacherDashboardResponse(
            new Stats(
                activeStudentsCount,
                activeLessonsCount,
                completedThisMonth,
                PercentChange(completedThisMonth, completedLastMonth),
                overallCompletionRate
            ),
            bestSales,
            logs,
            recentSales,
            new Attention(
                assignmentsToGrade,
                quizzesToGrade,
                unreadMessages,
                lessonsWithoutEnrollments,
                draftLessonsCount
            )
        );
    }

    private async Task<(int Assignments, int Quizzes, int Messages)> CountPendingWorkAsync(
        Guid? teacherId,
        CancellationToken cancellationToken
    )
    {
        var assignments = await unitOfWork
            .GetOrCreateRepository<AssignmentSubmission, int>()
            .CountAsync(new AssignmentsToGradeSpecification(teacherId), cancellationToken);

        var quizzes = await unitOfWork
            .GetOrCreateRepository<QuizAttempt, int>()
            .CountAsync(new QuizzesToGradeSpecification(teacherId), cancellationToken);

        var messages = await unitOfWork
            .GetOrCreateRepository<Conversation, Guid>()
            .CountAsync(new UnreadMessagesSpecification(teacherId.Value), cancellationToken);

        return (assignments, quizzes, messages);
    }

    private static decimal? PercentChange(decimal current, decimal previous) =>
        previous == 0 ? null : Math.Round((current - previous) / previous * 100, 1);

    private static DateOnly ToDate(DateTimeOffset value) =>
        DateOnly.FromDateTime(value.UtcDateTime);

    public sealed record EnrollmentInfo(
        DateTimeOffset? CreatedAt,
        decimal PaymentAmount,
        bool IsCompleted,
        DateTimeOffset? CompletedAt,
        int LessonId
    );

    public sealed record LessonDisplayInfo(int Id, string? Title);
}

internal class QuizzesToGradeSpecification : Specification<QuizAttempt>
{
    public QuizzesToGradeSpecification(Guid? teacherId)
    {
        Query.Where(qa =>
            qa.Status == QuizAttemptStatus.Submitted && qa.Quiz.TeacherId == teacherId
        );
    }
}

internal class AssignmentsToGradeSpecification : Specification<AssignmentSubmission>
{
    public AssignmentsToGradeSpecification(Guid? teacherId)
    {
        Query.Where(s => s.Score == null && s.Assignment.Lesson.TeacherId == teacherId);
    }
}

internal sealed class LessonsByIdsWithProjectionSpec<TResult> : Specification<Lesson, TResult>
{
    public LessonsByIdsWithProjectionSpec(
        IReadOnlyCollection<int> ids,
        Expression<Func<Lesson, TResult>> selector
    )
    {
        Query.Where(l => ids.Contains(l.Id));
        Query.Select(selector);
    }
}

internal class DraftLessonsSpecification : Specification<Lesson>
{
    public DraftLessonsSpecification(Guid? teacherId)
    {
        Query.Where(l => l.Status == LessonStatus.Drafted && l.TeacherId == teacherId);
    }
}

internal sealed record LessonSalesInfo(
    int Id,
    string? Title,
    int EnrollmentCount,
    int CompletedCount,
    decimal Amount
);

// Ranks the teacher's active lessons by enrollments entirely in SQL and returns only `take` rows.
internal sealed class TopSellingLessonsSpecification : Specification<Lesson, LessonSalesInfo>
{
    public TopSellingLessonsSpecification(Guid? teacherId, int take)
    {
        Query.Where(l =>
            l.TeacherId == teacherId && l.Status == LessonStatus.Active && l.Enrollments.Any()
        );

        Query.OrderByDescending(l => l.Enrollments.Count());
        Query.Take(take);

        Query.Select(l => new LessonSalesInfo(
            l.Id,
            l.Title,
            l.Enrollments.Count(),
            l.Enrollments.Count(e => e.IsCompleted),
            l.Enrollments.Sum(e => e.Payment != null ? e.Payment.Money.Amount : 0)
        ));
    }
}

// Use with CountAsync: all enrollments in the teacher's active lessons (or only the completed ones).
internal sealed class EnrollmentsOfActiveLessonsSpecification : Specification<Enrollment>
{
    public EnrollmentsOfActiveLessonsSpecification(Guid? teacherId, bool completedOnly = false)
    {
        Query.Where(e =>
            e.Lesson!.TeacherId == teacherId
            && e.Lesson!.Status == LessonStatus.Active
            && (!completedOnly || e.IsCompleted)
        );
    }
}

// Use with CountAsync: active lessons with no enrollment created since `since`.
internal sealed class LessonsWithoutRecentEnrollmentsSpecification : Specification<Lesson>
{
    public LessonsWithoutRecentEnrollmentsSpecification(Guid? teacherId, DateTimeOffset since)
    {
        Query.Where(l =>
            l.TeacherId == teacherId
            && l.Status == LessonStatus.Active
            && !l.Enrollments.Any(e => e.CreatedAt >= since)
        );
    }
}
