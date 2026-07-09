using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Quizzes;
using Prisma.Domain.Specifications.Users;

namespace Prisma.Application.Features.Users.Queries.GetAssistantProfile;

public class GetAssistantProfileQueryHandler(
    IUnitOfWork unitOfWork,
    UserManager<User> userManager)
    : IRequestHandler<GetAssistantProfileQuery, Result<RoleProfileDto>>
{
    public async Task<Result<RoleProfileDto>> Handle(GetAssistantProfileQuery request, CancellationToken cancellationToken)
    {
        var userRepo = unitOfWork.GetOrCreateRepository<User, Guid>();
        var user = await userRepo.FirstOrDefaultAsync(new UserByIdSpecification(request.AssistantId), cancellationToken);

        if (user is not Assistant assistant)
            throw new NotFoundException(nameof(Assistant), request.AssistantId);

        var now = DateTimeOffset.UtcNow;
        var weekStart = now.AddDays(-7);

        // NOTE: same platform-wide-KPI limitation as GetAssistantDashboardQueryHandler
        // (which this is modeled after) — there's no schema link between an
        // Assistant and a specific set of students/lessons to scope these by
        // (Assistant→Teacher has no FK; see AssistantConfiguration). Every
        // assistant's profile shows identical numbers here.
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var quizAttemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var auditRepo = unitOfWork.GetOrCreateRepository<AuditLog, int>();

        var activeStudents = await enrollmentRepo.CountAsync(new ActiveEnrollmentsSpec(), cancellationToken);
        var quizzesThisWeek = await quizAttemptRepo.CountAsync(new QuizAttemptsSpec(from: weekStart), cancellationToken);
        var totalLessons = await lessonRepo.CountAsync(new LessonsSpec(), cancellationToken);

        var stats = new List<ProfileStatDto>
        {
            new("الطلاب النشطون", activeStudents.ToString(), "text-[var(--purple-lt)]"),
            new("كويزات هذا الأسبوع", quizzesThisWeek.ToString(), "text-[var(--mint)]"),
            new("الدروس", totalLessons.ToString(), "text-[var(--star)]"),
        };

        // These two pieces ARE genuinely scoped to this specific assistant —
        // real per-target-user data, not inherited from the KPI limitation above.
        var logs = await auditRepo.ListAsync(
            new RecentAssistantLogsSpec(assistant.Email ?? string.Empty, take: 10),
            cancellationToken);

        var activities = logs
            .Select(l => new ProfileActivityDto(
                $"{l.Action} — {l.TableName}",
                l.CreatedAt?.ToString("yyyy-MM-dd hh:mm tt") ?? "—",
                "bg-[var(--star)]"))
            .ToList();

        var claims = await userManager.GetClaimsAsync(assistant);
        var heldPolicies = claims
            .Where(c => c.Type == AppClaims.PermissionsClaim)
            .Select(c => c.Value)
            .ToHashSet();

        var permissions = AppClaims.Policies.All
            .Select(policy => new ProfilePermissionDto(policy, heldPolicies.Contains(policy)))
            .ToList();

        var name = string.Join(" ", new[] { assistant.FirstName, assistant.SecondName, assistant.ThirdName, assistant.LastName }
            .Where(p => !string.IsNullOrWhiteSpace(p)));

        return Result<RoleProfileDto>.Success(new RoleProfileDto(name, stats, activities, permissions));
    }
}