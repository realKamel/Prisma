using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Features.Payments.HandleCallback;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Payments;
using Xunit;

namespace Prisma.Application.Tests.Features.Payments.Commands;

public class HandlePaymentCallbackCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Payment, int> _paymentRepo = Substitute.For<IRepository<Payment, int>>();
    private readonly IRepository<Enrollment, int> _enrollmentRepo = Substitute.For<IRepository<Enrollment, int>>();
    private readonly HandlePaymentCallbackCommandHandler _sut;

    public HandlePaymentCallbackCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Payment, int>().Returns(_paymentRepo);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _sut = new HandlePaymentCallbackCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenSuccessIsFalse_DoesNothing()
    {
        // Arrange
        var command = new HandlePaymentCallbackCommand("order-123", false,"123");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _paymentRepo.DidNotReceive().FirstOrDefaultAsync(Arg.Any<PaymentByProviderRefSpec>(), Arg.Any<CancellationToken>());
        _enrollmentRepo.DidNotReceive().Add(Arg.Any<Enrollment>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_DoesNothing()
    {
        // Arrange
        var command = new HandlePaymentCallbackCommand("order-123", true,"123");
        _paymentRepo.FirstOrDefaultAsync(Arg.Any<PaymentByProviderRefSpec>(), Arg.Any<CancellationToken>())
            .Returns((Payment?)null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _enrollmentRepo.DidNotReceive().Add(Arg.Any<Enrollment>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPaymentFound_MarksPaymentCompletedAndCreatesEnrollment()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = 42,
            StudentId = studentId,
            LessonId = 7,
            Status = PaymentStatus.Pending
        };

        var command = new HandlePaymentCallbackCommand("order-123", true, "123");
        _paymentRepo.FirstOrDefaultAsync(Arg.Any<PaymentByProviderRefSpec>(), Arg.Any<CancellationToken>())
            .Returns(payment);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.PaidAt.Should().NotBeNull();
        payment.PaidAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        _enrollmentRepo.Received(1).Add(Arg.Is<Enrollment>(e =>
            e.StudentId == studentId &&
            e.LessonId == 7 &&
            e.PaymentId == 42 &&
            e.EnrollmentMethod == EnrollmentMethod.OnlinePayment &&
            e.Status == EnrollmentStatus.Active));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}