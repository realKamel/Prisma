using MediatR;
using Ardalis.Result;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Admin;

namespace Prisma.Application.Features.Admin.Queries.GetAdminStatsQuery;

public class GetAdminStatsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAdminStatsQuery, Result<AdminStatsResponseDto>>
{
    public async Task<Result<AdminStatsResponseDto>> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        var studentRepo = _unitOfWork.GetOrCreateRepository<Student, Guid>();
        var paymentRepo = _unitOfWork.GetOrCreateRepository<Payment, int>();

        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);

        int totalStudents = await studentRepo.CountAsync(cancellationToken);
        var allStudents = await studentRepo.ListAsync(cancellationToken);
        int studentsThisMonth = allStudents.Count(s => s.CreatedAt >= startOfThisMonth);
        int studentsLastMonth = totalStudents - studentsThisMonth;

        decimal studentsDelta = 0;
        if (studentsLastMonth > 0)
        {
            studentsDelta = Math.Round(((decimal)(studentsThisMonth - studentsLastMonth) / studentsLastMonth) * 100, 1);
        }
        else if (studentsThisMonth > 0)
        {
            studentsDelta = 100;
        }


        var spec = new AdminSuccessfulPaymentsSpec();
        var successfulPayments = await paymentRepo.ListAsync(spec, cancellationToken);

        var paymentsThisMonth = successfulPayments
            .Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= startOfThisMonth).ToList();
        decimal revenueThisMonth = paymentsThisMonth.Sum(p => p.Amount);
        int lessonsSoldThisMonth = paymentsThisMonth.Count;

        var paymentsLastMonth = successfulPayments
            .Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= startOfLastMonth && p.PaidAt.Value < startOfThisMonth).ToList();
        decimal revenueLastMonth = paymentsLastMonth.Sum(p => p.Amount);
        int lessonsSoldLastMonth = paymentsLastMonth.Count;

        decimal revenueDelta = 0;
        if (revenueLastMonth > 0)
        {
            revenueDelta = Math.Round(((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100, 1);
        }
        else if (revenueThisMonth > 0)
        {
            revenueDelta = 100;
        }

        decimal lessonsDelta = 0;
        if (lessonsSoldLastMonth > 0)
        {
            lessonsDelta = Math.Round(((decimal)(lessonsSoldThisMonth - lessonsSoldLastMonth) / lessonsSoldLastMonth) * 100, 1);
        }
        else if (lessonsSoldThisMonth > 0)
        {
            lessonsDelta = 100;
        }

        var kpis = new List<KpiDto>
        {
            new("students", totalStudents, studentsDelta),
            new("revenue", successfulPayments.Sum(p => p.Amount), revenueDelta),
            new("lessons-sold", successfulPayments.Count, lessonsDelta),
            new("uptime", 99.9m, 0)
        };

        var revenueWeek = new List<RevenueWeekDto>();
        var today = now.Date;

        for (int i = 6; i >= 0; i--)
        {
            var currentDayDate = today.AddDays(-i);

            decimal dayAmount = successfulPayments
                .Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Date == currentDayDate)
                .Sum(p => p.Amount);

            revenueWeek.Add(new RevenueWeekDto(
                Date: new DateTimeOffset(currentDayDate, now.Offset),
                Amount: dayAmount,
                IsToday: currentDayDate == today
            ));
        }

        var response = new AdminStatsResponseDto(
            CurrentDateTime: now,
            Kpis: kpis,
            WeeklyTotal: revenueWeek.Sum(r => r.Amount),
            RevenueWeek: revenueWeek
        );

        return Result<AdminStatsResponseDto>.Success(response);
    }
}