using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Teachers.Queries.DTOs;
using Prisma.Application.Features.Teachers.Queries.GetTeacherDashboardStatus;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Tests.Features.Teacher.Queries;

public class GetTeacherDashboardStatusQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Enrollment, int> _enrollmentRepo = Substitute.For<
        IRepository<Enrollment, int>
    >();
    private readonly IRepository<Student, Guid> _studentRepo = Substitute.For<
        IRepository<Student, Guid>
    >();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<
        IRepository<Lesson, int>
    >();
    private readonly IRepository<AuditLog, long> _auditRepo = Substitute.For<
        IRepository<AuditLog, long>
    >();
    private readonly IRepository<User, Guid> _userRepo = Substitute.For<IRepository<User, Guid>>();
    private readonly GetTeacherDashboardStatusQueryHandler _sut;
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();

    public GetTeacherDashboardStatusQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepo);
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<AuditLog, long>().Returns(_auditRepo);
        _unitOfWork.GetOrCreateRepository<User, Guid>().Returns(_userRepo);

        _sut = new GetTeacherDashboardStatusQueryHandler(_unitOfWork, _currentUserService);

        // Defaults so every test doesn't have to stub every repo call
        _studentRepo
            .CountAsync(Arg.Any<ActiveStudentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _lessonRepo
            .CountAsync(Arg.Any<ActiveLessonsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _auditRepo
            .ListAsync(
                Arg.Any<PagedLogsOrderByCreatedAtSpec<AuditLogDto>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<AuditLogDto>());
        _enrollmentRepo
            .ListAsync(
                Arg.Any<
                    EnrollmentWithProjectionSpec<GetTeacherDashboardStatusQueryHandler.EnrollmentEarningInfo>
                >(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<GetTeacherDashboardStatusQueryHandler.EnrollmentEarningInfo>());
        _enrollmentRepo
            .ListAsync(
                Arg.Any<
                    EnrollmentWithProjectionSpec<GetTeacherDashboardStatusQueryHandler.EnrollmentLessonSaleInfo>
                >(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<GetTeacherDashboardStatusQueryHandler.EnrollmentLessonSaleInfo>());
    }

    private static GetTeacherDashboardStatusQueryHandler.EnrollmentEarningInfo Earning(
        DateTimeOffset? createdAt = null,
        decimal paymentAmount = 0m,
        bool isCompleted = false,
        DateTimeOffset? completedAt = null
    ) => new(createdAt, paymentAmount, isCompleted, completedAt);

    private void SetupEarnings(
        params GetTeacherDashboardStatusQueryHandler.EnrollmentEarningInfo[] earnings
    ) =>
        _enrollmentRepo
            .ListAsync(
                Arg.Any<
                    EnrollmentWithProjectionSpec<GetTeacherDashboardStatusQueryHandler.EnrollmentEarningInfo>
                >(),
                Arg.Any<CancellationToken>()
            )
            .Returns(earnings.ToList());

    private void SetupSales(
        params GetTeacherDashboardStatusQueryHandler.EnrollmentLessonSaleInfo[] sales
    ) =>
        _enrollmentRepo
            .ListAsync(
                Arg.Any<
                    EnrollmentWithProjectionSpec<GetTeacherDashboardStatusQueryHandler.EnrollmentLessonSaleInfo>
                >(),
                Arg.Any<CancellationToken>()
            )
            .Returns(sales.ToList());

    [Fact]
    public async Task Handle_ReturnsActiveStudentAndLessonCounts()
    {
        // Arrange
        _studentRepo
            .CountAsync(Arg.Any<ActiveStudentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(42);
        _lessonRepo
            .CountAsync(Arg.Any<ActiveLessonsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(17);

        var query = new GetTeacherDashboardStatusQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Stats.TotalActiveStudents.Should().Be(42);
        result.Value.Stats.TotalActiveLessons.Should().Be(17);
    }

    [Fact]
    public async Task Handle_ComputesThisMonthAndLastMonthEarningsWithGrowthPercentage()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        SetupEarnings(
            Earning(createdAt: now.AddDays(-10), paymentAmount: 200m),
            Earning(createdAt: now.AddDays(-40), paymentAmount: 100m)
        );

        var query = new GetTeacherDashboardStatusQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Stats.TotalEarningsForThisMonth.Should().Be(200m);
        result.Value.Stats.TotalEarningsAgainstLastMonth.Should().Be(200m); // (200/100)*100
    }

    [Fact]
    public async Task Handle_WhenLastMonthEarningIsZero_GrowthPercentageIsZero()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        SetupEarnings(Earning(createdAt: now.AddDays(-5), paymentAmount: 500m));

        var query = new GetTeacherDashboardStatusQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Stats.TotalEarningsAgainstLastMonth.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ComputesWeeklyEarningsGroupedByDayOfWeek()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var mondayThisWeek = now.AddDays(-2);
        SetupEarnings(
            Earning(createdAt: mondayThisWeek, paymentAmount: 50m),
            Earning(createdAt: mondayThisWeek, paymentAmount: 30m),
            Earning(createdAt: now.AddDays(-20), paymentAmount: 999m)
        );

        var query = new GetTeacherDashboardStatusQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.WeekEarnings.TotalEarningsForThisWeek.Should().Be(80m);
        result.Value.WeekEarnings.Data.Should().ContainSingle();
        result.Value.WeekEarnings.Data.Single().Earning.Should().Be(80m);
        result
            .Value.WeekEarnings.Data.Single()
            .Day.Should()
            .Be(mondayThisWeek.DayOfWeek.ToString());
    }

    [Fact]
    public async Task Handle_ComputesCompletedLessonCountsAndPercentageChange()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        SetupEarnings(
            Earning(createdAt: now.AddDays(-10), isCompleted: true, completedAt: now.AddDays(-5)),
            Earning(createdAt: now.AddDays(-8), isCompleted: true, completedAt: now.AddDays(-3)),
            Earning(createdAt: now.AddDays(-45), isCompleted: true, completedAt: now.AddDays(-40)),
            Earning(createdAt: now.AddDays(-10), isCompleted: false)
        );

        var query = new GetTeacherDashboardStatusQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Stats.TotalCompletedLessonsAgainstThisMonth.Should().Be(2);
        result.Value.Stats.TotalCompletedLessonsAgainstLastMonthPercentage.Should().Be(200m); // (2/1)*100
    }

    [Fact]
    public async Task Handle_ComputesTopThreeBestSellingLessonsByEnrollmentCount()
    {
        // Arrange
        SetupSales(
            new(1, 100m),
            new(1, 100m),
            new(1, 100m),
            new(2, 50m),
            new(2, 50m),
            new(3, 20m),
            new(4, 10m)
        );

        var query = new GetTeacherDashboardStatusQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.BestSales.Should().HaveCount(3);
        result.Value.BestSales[0].LessonId.Should().Be(1);
        result.Value.BestSales[0].StudentCount.Should().Be(3);
        result.Value.BestSales[0].Amount.Should().Be(300m);
        result.Value.BestSales[1].LessonId.Should().Be(2);
        result.Value.BestSales[2].LessonId.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ReturnsAuditLogsFromRepository()
    {
        // Arrange
        var logs = new List<AuditLogDto>
        {
            new(1, "teacher@example.com", "Update", "Lessons", DateTimeOffset.UtcNow),
        };
        _auditRepo
            .ListAsync(
                Arg.Any<PagedLogsOrderByCreatedAtSpec<AuditLogDto>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(logs);

        var query = new GetTeacherDashboardStatusQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Logs.Should().ContainSingle();
        result.Value.Logs.Single().Action.Should().Be("Update");
    }
}
