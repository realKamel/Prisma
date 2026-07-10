using FluentAssertions;
using NSubstitute;
using Prisma.Application.Features.Admin.Queries.GetAdminStatsQuery;
using Prisma.Application.Features.AdminDashboard.Queries.GetAdminStats;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Admin;

namespace Prisma.Application.Tests.Features.AdminDashboard.Queries;

public class GetAdminStatsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Student, Guid> _studentRepo = Substitute.For<IRepository<Student, Guid>>();
    private readonly IRepository<Payment, int> _paymentRepo = Substitute.For<IRepository<Payment, int>>();
    private readonly GetAdminStatsQueryHandler _sut;

    public GetAdminStatsQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepo);
        _unitOfWork.GetOrCreateRepository<Payment, int>().Returns(_paymentRepo);

        _sut = new GetAdminStatsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenDataExists_CalculatesKpisAndWeeklyRevenueCorrectly()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);

        // 1. إعداد الطلاب (طالب الشهر اللي فات وطالب الشهر ده)
        var fakeStudents = new List<Student>
        {
            new() { Id = Guid.CreateVersion7(), CreatedAt = startOfLastMonth.AddDays(2) },
            new() { Id = Guid.CreateVersion7(), CreatedAt = startOfThisMonth.AddDays(2) }
        };

        _studentRepo.CountAsync(Arg.Any<CancellationToken>()).Returns(fakeStudents.Count);
        _studentRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(fakeStudents);

        // 2. إعداد المدفوعات ناجحة
        var fakePayments = new List<Payment>
        {
            // دفعة الشهر الماضي
            new() { Id = 1, Amount = 100, PaidAt = startOfLastMonth.AddDays(5) },
            // دفعة اليوم (الشهر الحالي)
            new() { Id = 2, Amount = 200, PaidAt = now.Date.AddHours(10) }
        };

        _paymentRepo.ListAsync(Arg.Any<AdminSuccessfulPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakePayments);

        // Act
        var result = await _sut.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();

        // التحقق من الـ KPIs
        var studentsKpi = result.Data.Kpis.First(k => k.Id == "students");
        studentsKpi.Value.Should().Be(2);
        // طالب هذا الشهر (1) - طالب الشهر الماضي (1) / 1 * 100 = 0% زيادة صافية
        studentsKpi.Delta.Should().Be(0);

        var revenueKpi = result.Data.Kpis.First(k => k.Id == "revenue");
        revenueKpi.Value.Should().Be(300); // إجمالي الـ 100 + 200
        // دفعة الشهر ده (200) - دفعة الشهر اللي فات (100) / 100 * 100 = 100% زيادة
        revenueKpi.Delta.Should().Be(100);

        // التحقق من أسبوع الأرباح (RevenueWeek)
        result.Data.RevenueWeek.Should().HaveCount(7);
        var todayRevenue = result.Data.RevenueWeek.First(r => r.IsToday);
        todayRevenue.Amount.Should().Be(200); // دفعة اليوم

        result.Data.WeeklyTotal.Should().Be(200); // مجموع الـ 7 أيام الأخيرة (الـ 100 بتاعة الشهر اللي فات بره الأسبوع الحالي)
    }

    [Fact]
    public async Task Handle_WhenNoDataExists_ReturnsZeroedKpisAndUptime()
    {
        // Arrange
        _studentRepo.CountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _studentRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<Student>());
        _paymentRepo.ListAsync(Arg.Any<AdminSuccessfulPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        // Act
        var result = await _sut.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();

        var uptimeKpi = result.Data.Kpis.First(k => k.Id == "uptime");
        uptimeKpi.Value.Should().Be(99.9m);
        uptimeKpi.Delta.Should().Be(0);

        result.Data.WeeklyTotal.Should().Be(0);
        result.Data.RevenueWeek.Should().HaveCount(7);
        result.Data.RevenueWeek.All(r => r.Amount == 0).Should().BeTrue();
    }
}