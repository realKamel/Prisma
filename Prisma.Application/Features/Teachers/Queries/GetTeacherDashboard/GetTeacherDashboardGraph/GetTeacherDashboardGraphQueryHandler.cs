using System.Globalization;
using Ardalis.Result;
using MediatR;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherDashboard.GetTeacherDashboardGraph;

public class GetTeacherDashboardGraphQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeacherDashboardGraphQuery,
        Result<GetTeacherDashboardGraphResponse>>
{
    private const int PeriodDays = 30;
    private const int WindowDays = PeriodDays * 2; // 30 days + the 30 before them
    private static readonly TimeZoneInfo Cairo = ResolveCairo();

    public async Task<Result<GetTeacherDashboardGraphResponse>> Handle(
        GetTeacherDashboardGraphQuery request, CancellationToken cancellationToken)
    {
        var teacherId = request.TeacherId;

        // ---- Daily earnings (60 Cairo days, zero-filled) -----------------------------------------
        var now = DateTimeOffset.UtcNow;
        var queryFrom = now.AddDays(-(WindowDays + 1));
        var today = ToCairoDate(now);
        var windowStart = today.AddDays(-(WindowDays - 1));

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();

        var windowEnrollments = await enrollmentRepo.ListAsync(
            new EnrollmentWithProjectionSpec<EnrollmentInfo>(
                e => e.CreatedAt >= queryFrom
                     && teacherId == e.Lesson!.TeacherId,
                e => new EnrollmentInfo(
                    e.CreatedAt,
                    e.Payment != null ? e.Payment.Money.Amount : 0)),
            cancellationToken);

        var inWindow = windowEnrollments
            .Where(e => e.CreatedAt.HasValue)
            .Select(e => new { Info = e, Date = ToCairoDate(e.CreatedAt!.Value) })
            .Where(x => x.Date >= windowStart && x.Date <= today)
            .ToArray();

        var perDay = inWindow
            .GroupBy(x => x.Date)
            .ToDictionary(g => g.Key, g => (Earning: g.Sum(x => x.Info.PaymentAmount), Count: g.Count()));

        var daily = Enumerable.Range(0, WindowDays)
            .Select(i => windowStart.AddDays(i))
            .Select(d =>
            {
                perDay.TryGetValue(d, out var v);
                return new EarningEntry(d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), v.Earning, v.Count);
            })
            .ToArray();

        var thisMonthEarning = daily.TakeLast(PeriodDays).Sum(d => d.Earning);
        var lastMonthEarning = daily.Take(PeriodDays).Sum(d => d.Earning);

        return Result<GetTeacherDashboardGraphResponse>.Success(
            new GetTeacherDashboardGraphResponse(
                new GraphStats(thisMonthEarning, PercentChange(thisMonthEarning, lastMonthEarning)),
                new Earnings(daily)
            )
        );
    }

    private record EnrollmentInfo(DateTimeOffset? CreatedAt, decimal PaymentAmount);

    private static decimal? PercentChange(decimal current, decimal previous) =>
        previous == 0 ? null : Math.Round((current - previous) / previous * 100, 1);

    private static DateOnly ToCairoDate(DateTimeOffset value) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, Cairo).DateTime);

    private static TimeZoneInfo ResolveCairo()
    {
        foreach (var id in new[] { "Africa/Cairo", "Egypt Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                // try the next id (IANA on Linux, Windows id on older Windows hosts)
            }
        }

        return TimeZoneInfo.Utc;
    }
}