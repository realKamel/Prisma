using Ardalis.Specification;
using MediatR;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Responses.Generic;
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

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;

public class GetTeacherDashboardStatusQueryHandler(IUnitOfWork unitOfWork)
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

        var activeStudentsCount = await studentRepo.CountAsync(new ActiveStudentSpecification(), cancellationToken);
        var activeLessonsCount = await lessonRepo.CountAsync(new ActiveLessonsSpecification(), cancellationToken);

        var logs = (await auditRepo
        .ListAsync(
        new PagedLogsOrderByCreatedAtSpec<AuditLogDto>(l =>
        l.UserEmail != SystemConstants.SystemEmail,
        l => new AuditLogDto(l.Id, l.UserEmail, l.Action, l.TableName, l.CreatedAt),
        0,
        10),
        cancellationToken)).ToArray();

        var sixtyDayEnrollments = await enrollmentRepo.ListAsync(
            new EnrollmentWithPaymentOrderByCreatedAtDesc(e => e.CreatedAt >= now.AddDays(-60)),
            cancellationToken);

        var thisMonthEarning = sixtyDayEnrollments
            .Where(e => e.CreatedAt >= now.AddDays(-30))
            .Sum(e => e.Payment?.Amount ?? 0);

        var lastMonthEarning = sixtyDayEnrollments
            .Where(e => e.CreatedAt < now.AddDays(-30))
            .Sum(e => e.Payment?.Amount ?? 0);

        var totalEarningAgainstLastMonth = lastMonthEarning == 0
            ? 0
            : (thisMonthEarning / lastMonthEarning) * 100;

        var totalEarningThisWeek = sixtyDayEnrollments
            .Where(e => e.CreatedAt >= now.AddDays(-7))
            .Sum(e => e.Payment?.Amount ?? 0);

        var thisWeekEarning = sixtyDayEnrollments
            .Where(e => e.CreatedAt >= now.AddDays(-7))
            .GroupBy(e => e.CreatedAt!.Value.DayOfWeek)
            .Select(g =>
                new EarningEntry(g.Key.ToString(), g.Sum(d => d.Payment.Amount)))
            .ToArray();

        var completedThisMonth = sixtyDayEnrollments
            .Count(e => e.IsCompleted && e.CompletedAt >= now.AddDays(-30));

        var completedLastMonth = sixtyDayEnrollments
            .Count(e => e.IsCompleted && e.CompletedAt < now.AddDays(-30));

        var completedAgainstLastMonthPercentage = completedLastMonth == 0
            ? 0
            : ((decimal)completedThisMonth / completedLastMonth) * 100;

        var bestSalesEnrollments = await enrollmentRepo.ListAsync(
            new EnrollmentWithLessonAndPaymentOrderByCreatedAtDesc(e => e.Lesson.Status == LessonStatus.Active),
            cancellationToken);

        var bestSales = bestSalesEnrollments
            .GroupBy(e => e.Lesson)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new BestSales(
                g.Key.Id,
                g.Sum(e => e.Payment?.Amount ?? 0),
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
}