using Ardalis.Specification;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistantDashboard;

public class GetAssistantDashboardQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    UserManager<User> userManager)
    : IRequestHandler<GetAssistantDashboardQuery, Result<GetAssistantDashboardResponse>>
{
    // TODO: replace with actual teacher–assistant relation when modelled
    private const string SupervisorName = "أ. أحمد مصطفى";

    // Maps policy claim → permission tile id
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
        var now = DateTimeOffset.UtcNow;
        var weekStart = now.AddDays(-7);
        var prevWeekStart = now.AddDays(-14);

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var quizAttemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var submissionRepo = unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>();
        var auditRepo = unitOfWork.GetOrCreateRepository<AuditLog, int>();

        // ── KPI 1 · Active students ────────────────────────────
        var activeNow = await enrollmentRepo.CountAsync(new ActiveEnrollmentsSpec(), cancellationToken);
        var activeLastWeek = await enrollmentRepo.CountAsync(new ActiveEnrollmentsSpec(before: weekStart), cancellationToken);
        var studentDelta = activeNow - activeLastWeek;

        // ── KPI 2 · Quizzes this week + pass-rate delta ────────
        var quizzesThisWeek = await quizAttemptRepo.CountAsync(new QuizAttemptsSpec(from: weekStart), cancellationToken);

        var gradedThisWeek = await quizAttemptRepo.ListAsync(new QuizAttemptsSpec(from: weekStart, to: now, status: QuizAttemptStatus.Graded), cancellationToken);
        var gradedLastWeek = await quizAttemptRepo.ListAsync(new QuizAttemptsSpec(from: prevWeekStart, to: weekStart, status: QuizAttemptStatus.Graded), cancellationToken);

        var passRateDelta = ComputePassRate(gradedThisWeek) - ComputePassRate(gradedLastWeek);

        // ── KPI 3 · Ungraded submissions ───────────────────────
        var ungradedSubmissions = await submissionRepo.CountAsync(new UngradedSubmissionsSpec(), cancellationToken);

        // ── KPI 4 · Lessons ────────────────────────────────────
        var totalLessons = await lessonRepo.CountAsync(new LessonsSpec(), cancellationToken);
        var newLessonsThisWeek = await lessonRepo.CountAsync(new LessonsSpec(from: weekStart), cancellationToken);

        // ── Activities ─────────────────────────────────────────
        var logs = await auditRepo.ListAsync(
            new RecentAssistantLogsSpec(currentUser.Email!, take: 10),
            cancellationToken);

        var activities = logs
            .Select(l => new ActivityItem(l.Id, l.Action, l.TableName, l.CreatedAt ?? DateTimeOffset.UtcNow))
            .ToList();

        // ── Permissions ────────────────────────────────────────
        var assistant = await userManager.FindByIdAsync(currentUser.UserId!.Value.ToString());
        var claims = assistant is not null
            ? await userManager.GetClaimsAsync(assistant)
            : [];

        var heldPolicies = claims
            .Where(c => c.Type == AppClaims.PermissionsClaim)
            .Select(c => c.Value)
            .ToHashSet();

        var permissions = PermissionMap
            .Select(kvp => new Permission(
                Id: kvp.Value,
                Status: heldPolicies.Contains(kvp.Key) ? "on" : "off"))
            .ToList();

        // ── Assemble ───────────────────────────────────────────
        var response = new GetAssistantDashboardResponse
        {
            Teacher = new DashboardTeacher(
                Name: assistant is not null
                                    ? $"{assistant.FirstName} {assistant.LastName}"
                                    : string.Empty,
                SupervisorName: SupervisorName),

            Kpis =
            [
                new("students",    activeNow,           studentDelta,       studentDelta       >= 0 ? "up" : "down", "purple"),
                new("quizzes",     quizzesThisWeek,     passRateDelta,      passRateDelta      >= 0 ? "up" : "down", "mint"),
                new("assignments", ungradedSubmissions, 0,                  "down",                                  "star"),
                new("lessons",     totalLessons,        newLessonsThisWeek, newLessonsThisWeek  > 0 ? "up" : "down", "coral"),
            ],

            Activities = activities,
            Permissions = permissions,
        };

        return Result<GetAssistantDashboardResponse>.Success(response);
    }

    private static double ComputePassRate(IList<QuizAttempt> attempts)
    {
        if (attempts.Count == 0) return 0.0;
        var passed = attempts.Count(a => a.Degree >= a.Quiz.TotalDegree * 0.5m);
        return passed / (double)attempts.Count;
    }
}