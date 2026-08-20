using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Users.Queries.GetAssistantProfile;

public class GetAssistantProfileQueryHandler(
    IUnitOfWork unitOfWork,
    UserManager<User> userManager)
    : IRequestHandler<GetAssistantProfileQuery, Result<RoleProfileDto>>
{
    public async Task<Result<RoleProfileDto>> Handle(GetAssistantProfileQuery request, CancellationToken cancellationToken)
    {
        var assistantRepo = unitOfWork.GetOrCreateRepository<Assistant, Guid>();
        var assistant = await assistantRepo.GetByIdAsync(request.AssistantId, cancellationToken);

        if (assistant is null)
            return Result.NotFound($"Assistant with id '{request.AssistantId}' was not found");

        var teacherId = assistant.TeacherId!.Value; 

        var now = DateTimeOffset.UtcNow;
        var weekStart = now.AddDays(-7);

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var quizAttemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var auditRepo = unitOfWork.GetOrCreateRepository<AuditLog, int>();

        var activeStudentsTask = await enrollmentRepo.CountAsync(
            new ActiveEnrollmentsSpec(teacherId), cancellationToken);
        var quizzesThisWeekTask = await quizAttemptRepo.CountAsync(
            new QuizAttemptsSpec(teacherId, from: weekStart), cancellationToken);
        var totalLessonsTask = await lessonRepo.CountAsync(
            new LessonsSpec(teacherId), cancellationToken);
        var logsTask = await auditRepo.ListAsync(
            new RecentAssistantLogsSpec(assistant.Email ?? string.Empty, take: 10), cancellationToken);

        var stats = new List<ProfileStatDto>
        {
            new("الطلاب النشطون", activeStudentsTask.ToString(), "text-[var(--purple-lt)]"),
            new("كويزات هذا الأسبوع", quizzesThisWeekTask.ToString(), "text-[var(--mint)]"),
            new("الدروس", totalLessonsTask.ToString(), "text-[var(--star)]"),
        };

        var activities = logsTask
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