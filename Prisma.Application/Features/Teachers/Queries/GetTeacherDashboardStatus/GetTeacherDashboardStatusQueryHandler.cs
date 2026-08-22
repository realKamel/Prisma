using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Teachers.Queries.DTOs;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Students;
using Prisma.Domain.Specifications.Users;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;

public class GetTeacherDashboardStatusQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetTeacherDashboardStatusQuery,
        Result<GetTeacherDashboardStatusResponse>>
{
    public async Task<Result<GetTeacherDashboardStatusResponse>> Handle(
        GetTeacherDashboardStatusQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var auditRepo = unitOfWork.GetOrCreateRepository<AuditLog, int>();
        var userRepo = unitOfWork.GetOrCreateRepository<User, Guid>();

        var teacherId = request.TeacherId ?? currentUserService.UserId;
        string? teacherEmail = currentUserService.Email;

        if (request.TeacherId.HasValue && request.TeacherId != currentUserService.UserId)
        {
            teacherEmail = await userRepo.FirstOrDefaultAsync(
                new UserWithProjectionSpec<string?>(request.TeacherId.Value, u => u.Email),
                cancellationToken);
        }

        var logsSpec = new PagedLogsOrderByCreatedAtSpec<AuditLogDto>(
            l => l.UserEmail != SystemConstants.SystemEmail
                 && (teacherEmail == null || l.UserEmail == teacherEmail),
            l => new AuditLogDto(l.Id, l.UserEmail, l.Action, l.TableName, l.CreatedAt),
            0,
            10);


        var activeStudentsCount = await studentRepo.CountAsync(
            new ActiveStudentSpecification(teacherId), cancellationToken);

        var activeLessonsCount = await lessonRepo.CountAsync(
            new ActiveLessonsSpecification(teacherId), cancellationToken); 

        var logs = (await auditRepo.ListAsync(logsSpec, cancellationToken)).ToArray();

        var sixtyDayEnrollments = await enrollmentRepo.ListAsync(
            new EnrollmentWithProjectionSpec<EnrollmentEarningInfo>(
                e => e.CreatedAt >= now.AddDays(-60)
                     && (teacherId == null || e.Student.TeacherStudents.Any(ts => ts.TeacherId == teacherId && !ts.IsKicked)),
                e => new EnrollmentEarningInfo(
                    e.CreatedAt,
                    e.Payment != null ? e.Payment.Amount : 0,
                    e.IsCompleted,
                    e.CompletedAt)),
            cancellationToken);

        var bestSalesEnrollments = await enrollmentRepo.ListAsync(
            new EnrollmentWithProjectionSpec<EnrollmentLessonSaleInfo>(
                e => e.Lesson!.Status == LessonStatus.Active
                     && (teacherId == null || e.Student.TeacherStudents.Any(ts => ts.TeacherId == teacherId && !ts.IsKicked)),
                e => new EnrollmentLessonSaleInfo(
                    e.Lesson!.Id,
                    e.Payment != null ? e.Payment.Amount : 0)),
            cancellationToken);

        var thisMonthEarning = sixtyDayEnrollments
            .Where(e => e.CreatedAt >= now.AddDays(-30))
            .Sum(e => e.PaymentAmount);

        var lastMonthEarning = sixtyDayEnrollments
            .Where(e => e.CreatedAt < now.AddDays(-30))
            .Sum(e => e.PaymentAmount);

        var totalEarningAgainstLastMonth = lastMonthEarning == 0
            ? 0
            : (thisMonthEarning / lastMonthEarning) * 100;

        var totalEarningThisWeek = sixtyDayEnrollments
            .Where(e => e.CreatedAt >= now.AddDays(-7))
            .Sum(e => e.PaymentAmount);

        var thisWeekEarning = sixtyDayEnrollments
            .Where(e => e.CreatedAt >= now.AddDays(-7))
            .GroupBy(e => e.CreatedAt?.DayOfWeek)
            .Select(g => new EarningEntry(g.Key.ToString(), g.Sum(d => d.PaymentAmount)))
            .ToArray();

        var completedThisMonth = sixtyDayEnrollments
            .Count(e => e.IsCompleted && e.CompletedAt >= now.AddDays(-30));

        var completedLastMonth = sixtyDayEnrollments
            .Count(e => e.IsCompleted && e.CompletedAt < now.AddDays(-30));

        var completedAgainstLastMonthPercentage = completedLastMonth == 0
            ? 0
            : ((decimal)completedThisMonth / completedLastMonth) * 100;

        var bestSales = bestSalesEnrollments
            .GroupBy(e => e.LessonId)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new BestSales(
                g.Key,
                g.Sum(e => e.PaymentAmount),
                g.Count()))
            .ToArray();

        return new GetTeacherDashboardStatusResponse(
            new Stats(
                thisMonthEarning,
                totalEarningAgainstLastMonth,
                activeStudentsCount,
                activeLessonsCount,
                completedThisMonth,
                completedAgainstLastMonthPercentage),
            new WeekEarnings(totalEarningThisWeek, thisWeekEarning),
            bestSales,
            logs);
    }
    public sealed record EnrollmentEarningInfo(
        DateTimeOffset? CreatedAt,
        decimal PaymentAmount,
        bool IsCompleted,
        DateTimeOffset? CompletedAt
    );

    public sealed record EnrollmentLessonSaleInfo(int LessonId, decimal PaymentAmount);
}