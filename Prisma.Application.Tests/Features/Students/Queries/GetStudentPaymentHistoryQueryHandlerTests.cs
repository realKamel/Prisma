using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;
using Prisma.Application.Features.Students.Queries.GetStudentPaymentHistoryQuery;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;

namespace Prisma.Application.Tests.Features.Students.Queries;

public class GetStudentPaymentHistoryQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();
    private readonly IRepository<Enrollment, int> _enrollmentRepo = Substitute.For<
        IRepository<Enrollment, int>
    >();
    private readonly GetStudentPaymentHistoryQueryHandler _sut;

    public GetStudentPaymentHistoryQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _sut = new GetStudentPaymentHistoryQueryHandler(_unitOfWork, _currentUserService);
    }

    private static GetStudentPaymentHistoryQueryHandler.EnrollmentInfo Info(
        int id = 1,
        EnrollmentMethod enrollmentMethod = EnrollmentMethod.RedeemCode,
        PaymentStatus paymentStatus = PaymentStatus.Pending,
        DateTimeOffset? expiresAt = null,
        string? paymentProvider = null,
        decimal? paymentAmount = null,
        DateTimeOffset? paymentPaidAt = null,
        DateTimeOffset? paymentCreatedAt = null,
        DateTimeOffset? createdAt = null,
        string? paymentProviderRef = null,
        int? paymentId = null,
        string? lessonTitle = "Physics 101",
        Guid? lessonPublicId = null
    ) =>
        new(
            Id: id,
            EnrollmentMethod: enrollmentMethod,
            PaymentStatus: paymentStatus,
            ExpiresAt: expiresAt,
            PaymentProvider: paymentProvider,
            PaymentAmount: paymentAmount,
            PaymentPaidAt: paymentPaidAt,
            PaymentCreatedAt: paymentCreatedAt,
            CreatedAt: createdAt,
            PaymentProviderRef: paymentProviderRef,
            PaymentId: paymentId,
            LessonTitle: lessonTitle,
            LessonPublicId: lessonPublicId ?? Guid.NewGuid()
        );

    private void SetupEnrollments(
        params GetStudentPaymentHistoryQueryHandler.EnrollmentInfo[] enrollments
    )
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _enrollmentRepo
            .ListAsync(
                Arg.Any<
                    EnrollmentWithProjectionSpec<GetStudentPaymentHistoryQueryHandler.EnrollmentInfo>
                >(),
                Arg.Any<CancellationToken>()
            )
            .Returns(enrollments.ToList());
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetStudentPaymentHistoryQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        await _enrollmentRepo
            .DidNotReceive()
            .ListAsync(
                Arg.Any<
                    EnrollmentWithProjectionSpec<GetStudentPaymentHistoryQueryHandler.EnrollmentInfo>
                >(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenEnrollmentHasNoQualifyingPayment_ExcludesItFromResults()
    {
        // Arrange: not redeem code, no completed payment, not a teacher grant -> should be skipped
        SetupEnrollments(
            Info(
                enrollmentMethod: EnrollmentMethod.OnlinePayment,
                paymentStatus: PaymentStatus.Pending
            )
        );

        // Act
        var result = await _sut.Handle(new GetStudentPaymentHistoryQuery(), CancellationToken.None);

        // Assert
        result.Value!.Payments.Should().BeEmpty();
        result.Value.Stats.LessonsPurchased.Should().Be(0);
        result.Value.Stats.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentPaidViaRedeemCode_MapsToCodeMethodWithZeroAmount()
    {
        // Arrange
        var lessonPublicId = Guid.NewGuid();
        SetupEnrollments(
            Info(
                enrollmentMethod: EnrollmentMethod.RedeemCode,
                createdAt: DateTimeOffset.UtcNow.AddDays(-2),
                lessonTitle: "Physics 101",
                lessonPublicId: lessonPublicId
            )
        );

        // Act
        var result = await _sut.Handle(new GetStudentPaymentHistoryQuery(), CancellationToken.None);

        // Assert
        result.Value!.Payments.Should().ContainSingle();
        var dto = result.Value.Payments.Single();
        dto.Method.Should().Be("CODE");
        dto.Amount.Should().Be(0);
        dto.LessonTitle.Should().Be("Physics 101");
        dto.LessonId.Should().Be(lessonPublicId);
        dto.Status.Should().Be("paid");
    }

    [Fact]
    public async Task Handle_WhenEnrollmentPaidOnlineWithMatchingProvider_MapsToOnlineMethodWithAmount()
    {
        // Arrange
        SetupEnrollments(
            Info(
                enrollmentMethod: EnrollmentMethod.OnlinePayment,
                paymentStatus: PaymentStatus.Completed,
                paymentProvider: "Online",
                paymentAmount: 250m,
                paymentProviderRef: "ref-123",
                paymentPaidAt: DateTimeOffset.UtcNow.AddDays(-1)
            )
        );

        // Act
        var result = await _sut.Handle(new GetStudentPaymentHistoryQuery(), CancellationToken.None);

        // Assert
        var dto = result.Value!.Payments.Single();
        dto.Method.Should().Be("ONLINE");
        dto.Amount.Should().Be(250m);
        dto.Id.Should().Be("ref-123");
    }

    [Fact]
    public async Task Handle_WhenEnrollmentIsTeacherGrant_MapsToTeacherGrantMethod()
    {
        // Arrange
        SetupEnrollments(
            Info(enrollmentMethod: EnrollmentMethod.TeacherGrant, createdAt: DateTimeOffset.UtcNow)
        );

        // Act
        var result = await _sut.Handle(new GetStudentPaymentHistoryQuery(), CancellationToken.None);

        // Assert
        var dto = result.Value!.Payments.Single();
        dto.Method.Should().Be("TEACHER_GRANT");
        dto.Amount.Should().Be(0);
        dto.Status.Should().Be("paid");
    }

    [Fact]
    public async Task Handle_WhenEnrollmentExpired_SetsStatusToExpired()
    {
        // Arrange
        SetupEnrollments(
            Info(
                enrollmentMethod: EnrollmentMethod.RedeemCode,
                expiresAt: DateTimeOffset.UtcNow.AddDays(-1),
                createdAt: DateTimeOffset.UtcNow.AddDays(-10)
            )
        );

        // Act
        var result = await _sut.Handle(new GetStudentPaymentHistoryQuery(), CancellationToken.None);

        // Assert
        result.Value!.Payments.Single().Status.Should().Be("expired");
    }

    [Fact]
    public async Task Handle_WhenPaymentReferenceMissing_FallsBackToEnrollmentId()
    {
        // Arrange
        SetupEnrollments(
            Info(id: 42, enrollmentMethod: EnrollmentMethod.TeacherGrant, paymentId: null)
        );

        // Act
        var result = await _sut.Handle(new GetStudentPaymentHistoryQuery(), CancellationToken.None);

        // Assert
        result.Value!.Payments.Single().Id.Should().Be("enr-42");
    }

    [Fact]
    public async Task Handle_UsesPaymentPaidAtThenPaymentCreatedAtThenEnrollmentCreatedAt()
    {
        // Arrange
        var paidAt = new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero);
        var paymentCreatedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);
        var enrollmentCreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero);

        SetupEnrollments(
            Info(
                id: 1,
                enrollmentMethod: EnrollmentMethod.TeacherGrant,
                paymentPaidAt: paidAt,
                paymentCreatedAt: paymentCreatedAt,
                createdAt: enrollmentCreatedAt
            ),
            Info(
                id: 2,
                enrollmentMethod: EnrollmentMethod.TeacherGrant,
                paymentPaidAt: null,
                paymentCreatedAt: paymentCreatedAt,
                createdAt: enrollmentCreatedAt
            ),
            Info(
                id: 3,
                enrollmentMethod: EnrollmentMethod.TeacherGrant,
                paymentPaidAt: null,
                paymentCreatedAt: null,
                createdAt: enrollmentCreatedAt
            )
        );

        // Act
        var result = await _sut.Handle(new GetStudentPaymentHistoryQuery(), CancellationToken.None);

        // Assert
        var payments = result.Value!.Payments.OrderBy(p => p.Id).ToList();
        payments[0].PaymentDate.Should().Be(paidAt);
        payments[1].PaymentDate.Should().Be(paymentCreatedAt);
        payments[2].PaymentDate.Should().Be(enrollmentCreatedAt);
    }

    [Fact]
    public async Task Handle_ComputesAggregateStatsAcrossMultipleEnrollments()
    {
        // Arrange
        var activeCodeEnrollment = Info(
            id: 1,
            enrollmentMethod: EnrollmentMethod.RedeemCode,
            createdAt: DateTimeOffset.UtcNow
        );
        var activeOnlineEnrollment = Info(
            id: 2,
            enrollmentMethod: EnrollmentMethod.OnlinePayment,
            paymentStatus: PaymentStatus.Completed,
            paymentProvider: "online",
            paymentAmount: 300m
        );
        var expiredEnrollment = Info(
            id: 3,
            enrollmentMethod: EnrollmentMethod.RedeemCode,
            expiresAt: DateTimeOffset.UtcNow.AddDays(-5),
            createdAt: DateTimeOffset.UtcNow.AddDays(-30)
        );
        var excludedEnrollment = Info(
            id: 4,
            enrollmentMethod: EnrollmentMethod.OnlinePayment,
            paymentStatus: PaymentStatus.Pending,
            paymentProvider: "online",
            paymentAmount: 500m
        );

        SetupEnrollments(
            activeCodeEnrollment,
            activeOnlineEnrollment,
            expiredEnrollment,
            excludedEnrollment
        );

        // Act
        var result = await _sut.Handle(new GetStudentPaymentHistoryQuery(), CancellationToken.None);

        // Assert
        result.Value!.Stats.LessonsPurchased.Should().Be(3);
        result.Value.Stats.ActiveLessons.Should().Be(2);
        result.Value.Stats.ExpiredLessons.Should().Be(1);
        result.Value.Stats.TotalAmount.Should().Be(300m);
    }
}
