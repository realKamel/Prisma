using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assignments;
using Prisma.Domain.Specifications.AuditLogs;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.QuizAttemptSpecs;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistantDashboard;

public class GetAssistantDashboardQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IIdentityService userManager)
    : IRequestHandler<GetAssistantDashboardQuery, Result<GetAssistantDashboardResponse>>
{
    private static readonly IReadOnlyDictionary<string, string> PermissionMap =
        new Dictionary<string, string>
        {
            [AppClaims.Policies.CanManageEnrollments] = "students",
            [AppClaims.Policies.CanManageContent] = "content",
            [AppClaims.Policies.CanViewReports] = "reports",
            [AppClaims.Policies.CanEvaluateStudents] = "grading",
        };

    public async Task<Result<GetAssistantDashboardResponse>> Handle(
        GetAssistantDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result<GetAssistantDashboardResponse>.Unauthorized();

        var assistant = await userManager.FindByIdAsync(userId);
        if (assistant is not Assistant assistantUser)
            return Result<GetAssistantDashboardResponse>.Unauthorized();

        var teacherId = assistantUser.TeacherId!.Value;

        var teacher = await userManager.FindByIdAsync(teacherId);

        var claims = await userManager.GetClaimsAsync(assistantUser);
        var heldPolicies = claims
            .Where(c => c.Type == AppClaims.PermissionsClaim)
            .Select(c => c.Value)
            .ToHashSet();

        var permissions = PermissionMap
            .Select(kvp => new Permission(
                Id: kvp.Value,
                Status: heldPolicies.Contains(kvp.Key) ? "on" : "off"))
            .ToList();

        var now = DateTimeOffset.UtcNow;
        var weekStart = now.AddDays(-7);
        var prevWeekStart = now.AddDays(-14);

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var quizAttemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var submissionRepo = unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>();
        var auditRepo = unitOfWork.GetOrCreateRepository<AuditLog, int>();

        // ── KPI 1 · Active students ────────────────────────────
        var activeNow = await enrollmentRepo.CountAsync(
            new ActiveEnrollmentsSpec(teacherId), cancellationToken);
        var activeLastWeek = await enrollmentRepo.CountAsync(
            new ActiveEnrollmentsSpec(teacherId, before: weekStart), cancellationToken);
        var studentDelta = activeNow - activeLastWeek;

        // ── KPI 2 · Quizzes this week + pass-rate delta ────────
        var quizzesThisWeek = await quizAttemptRepo.CountAsync(
            new QuizAttemptsSpec(teacherId, from: weekStart), cancellationToken);

        var gradedThisWeek = await quizAttemptRepo.ListAsync(
            new QuizAttemptWithProjectionSpec<QuizAttemptScoreInfo>(
                teacherId, weekStart, now, QuizAttemptStatus.Graded,
                a => new QuizAttemptScoreInfo(a.Degree, a.Quiz!.TotalDegree)),
            cancellationToken);

        var gradedLastWeek = await quizAttemptRepo.ListAsync(
            new QuizAttemptWithProjectionSpec<QuizAttemptScoreInfo>(
                teacherId, prevWeekStart, weekStart, QuizAttemptStatus.Graded,
                a => new QuizAttemptScoreInfo(a.Degree, a.Quiz!.TotalDegree)),
            cancellationToken);

        var passRateDelta = ComputePassRate(gradedThisWeek) - ComputePassRate(gradedLastWeek);

        // ── KPI 3 · Ungraded submissions ───────────────────────
        var ungradedSubmissions = await submissionRepo.CountAsync(
            new UngradedSubmissionsSpec(teacherId), cancellationToken);

        // ── KPI 4 · Lessons ────────────────────────────────────
        var totalLessons = await lessonRepo.CountAsync(
            new LessonsSpec(teacherId), cancellationToken);
        var newLessonsThisWeek = await lessonRepo.CountAsync(
            new LessonsSpec(teacherId, from: weekStart), cancellationToken);

        // ── Activities ─────────────────────────────────────────
        var activities = await auditRepo.ListAsync(
            new AuditLogWithProjectionSpec<ActivityItem>(
                currentUser.Email!, take: 10,
                l => new ActivityItem(l.Id, l.Action, l.TableName, l.CreatedAt ?? DateTimeOffset.UtcNow)),
            cancellationToken);

        // ── Assemble ───────────────────────────────────────────
        var response = new GetAssistantDashboardResponse
        {
            Teacher = new DashboardTeacher(
                Name: $"{assistantUser.FirstName} {assistantUser.SecondName}",
                SupervisorName: teacher is not null
                    ? $"{teacher.FirstName} {teacher.SecondName}"
                    : string.Empty),

            Kpis =
            [
                new("students", activeNow, studentDelta, studentDelta >= 0 ? "up" : "down", "purple"),
                new("quizzes", quizzesThisWeek, passRateDelta, passRateDelta >= 0 ? "up" : "down", "mint"),
                new("assignments", ungradedSubmissions, 0, "down", "star"),
                new("lessons", totalLessons, newLessonsThisWeek, newLessonsThisWeek > 0 ? "up" : "down", "coral"),
            ],

            Activities = activities.ToList(),
            Permissions = permissions,
        };

        return Result<GetAssistantDashboardResponse>.Success(response);
    }

    private static double ComputePassRate(IList<QuizAttemptScoreInfo> attempts)
    {
        if (attempts.Count == 0) return 0.0;
        var passed = attempts.Count(a => a.Degree >= a.TotalDegree * 0.5m);
        return passed / (double)attempts.Count;
    }
    public sealed record QuizAttemptScoreInfo(decimal Degree, decimal TotalDegree);
}