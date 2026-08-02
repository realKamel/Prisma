using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Tests.Features.Students.Queries;

public class GetStudentPaymentHistoryQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IRepository<Enrollment, int> _enrollmentRepo = Substitute.For<IRepository<Enrollment, int>>();
    private readonly GetStudentPaymentHistoryQueryHandler _sut;

    public GetStudentPaymentHistoryQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _sut = new GetStudentPaymentHistoryQueryHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetStudentPaymentHistoryQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenEnrollmentHasNoQualifyingPayment_ExcludesItFromResults()
    {
        // Arrange: not redeem code, no completed payment, not a teacher grant -> should be skipped
        var userId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = 1,
            EnrollmentMethod = EnrollmentMethod.OnlinePayment,
            Payment = new Payment { Status = PaymentStatus.Pending, Amount = 100m, Provider = "online" }
        };

        _currentUserService.UserId.Returns(userId);
        _enrollmentRepo.ListAsync(Arg.Any<StudentEnrollmentsWithPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment> { enrollment });

        var query = new GetStudentPaymentHistoryQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Payments.Should().BeEmpty();
        result.Value.Stats.LessonsPurchased.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentPaidViaRedeemCode_MapsToCodeMethodWithZeroAmount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lesson = new Lesson { Id = 10, Title = "Physics 101" };
        var enrollment = new Enrollment
        {
            Id = 1,
            LessonId = 10,
            Lesson = lesson,
            EnrollmentMethod = EnrollmentMethod.RedeemCode,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
            Payment = null
        };

        _currentUserService.UserId.Returns(userId);
        _enrollmentRepo.ListAsync(Arg.Any<StudentEnrollmentsWithPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment> { enrollment });

        var query = new GetStudentPaymentHistoryQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Payments.Should().ContainSingle();
        var dto = result.Value.Payments.Single();
        dto.Method.Should().Be("code");
        dto.Amount.Should().Be(0);
        dto.LessonTitle.Should().Be("Physics 101");
        dto.Status.Should().Be("paid");
    }

    [Fact]
    public async Task Handle_WhenEnrollmentPaidOnlineWithMatchingProvider_MapsToOnlineMethodWithAmount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var payment = new Payment
        {
            Status = PaymentStatus.Completed,
            Amount = 250m,
            Provider = "Online",
            ProviderRef = "ref-123",
            PaidAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        var enrollment = new Enrollment
        {
            Id = 1,
            EnrollmentMethod = EnrollmentMethod.OnlinePayment,
            Payment = payment
        };

        _currentUserService.UserId.Returns(userId);
        _enrollmentRepo.ListAsync(Arg.Any<StudentEnrollmentsWithPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment> { enrollment });

        var query = new GetStudentPaymentHistoryQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Value!.Payments.Single();
        dto.Method.Should().Be("online");
        dto.Amount.Should().Be(250m);
        dto.Id.Should().Be("ref-123");
    }

    [Fact]
    public async Task Handle_WhenEnrollmentIsTeacherGrant_MapsToTeacherGrantMethod()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = 1,
            EnrollmentMethod = EnrollmentMethod.TeacherGrant,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _currentUserService.UserId.Returns(userId);
        _enrollmentRepo.ListAsync(Arg.Any<StudentEnrollmentsWithPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment> { enrollment });

        var query = new GetStudentPaymentHistoryQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Value!.Payments.Single();
        dto.Method.Should().Be("teacher grant");
        dto.Amount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentExpired_SetsStatusToExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var enrollment = new Enrollment
        {
            Id = 1,
            EnrollmentMethod = EnrollmentMethod.RedeemCode,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        _currentUserService.UserId.Returns(userId);
        _enrollmentRepo.ListAsync(Arg.Any<StudentEnrollmentsWithPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment> { enrollment });

        var query = new GetStudentPaymentHistoryQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Payments.Single().Status.Should().Be("expired");
    }

    [Fact]
    public async Task Handle_ComputesAggregateStatsAcrossMultipleEnrollments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeCodeEnrollment = new Enrollment
        {
            Id = 1,
            EnrollmentMethod = EnrollmentMethod.RedeemCode,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var activeOnlineEnrollment = new Enrollment
        {
            Id = 2,
            EnrollmentMethod = EnrollmentMethod.OnlinePayment,
            Payment = new Payment { Status = PaymentStatus.Completed, Amount = 300m, Provider = "online" }
        };
        var expiredEnrollment = new Enrollment
        {
            Id = 3,
            EnrollmentMethod = EnrollmentMethod.RedeemCode,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-5),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-30)
        };
        var excludedEnrollment = new Enrollment
        {
            Id = 4,
            EnrollmentMethod = EnrollmentMethod.OnlinePayment,
            Payment = new Payment { Status = PaymentStatus.Pending, Amount = 500m, Provider = "online" }
        };

        _currentUserService.UserId.Returns(userId);
        _enrollmentRepo.ListAsync(Arg.Any<StudentEnrollmentsWithPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment>
            {
                activeCodeEnrollment, activeOnlineEnrollment, expiredEnrollment, excludedEnrollment
            });

        var query = new GetStudentPaymentHistoryQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value!.Stats.LessonsPurchased.Should().Be(3);
        result.Value.Stats.ActiveLessons.Should().Be(2);
        result.Value.Stats.ExpiredLessons.Should().Be(1);
        result.Value.Stats.TotalAmount.Should().Be(300m);
    }
}