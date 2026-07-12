using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Prisma.Application.Features.Payments.InitiatePayment;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Xunit;

namespace Prisma.Application.Tests.Features.Payments.InitiatePayment;

public class InitiatePaymentCommandHandlerTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyedServiceProvider _keyedServiceProvider;
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Payment, int> _paymentRepo = Substitute.For<IRepository<Payment, int>>();
    private readonly IPaymentService _paymentService = Substitute.For<IPaymentService>();
    private readonly InitiatePaymentCommandHandler _sut;

    public InitiatePaymentCommandHandlerTests()
    {
        // IServiceProvider must also implement IKeyedServiceProvider since
        // GetRequiredKeyedService<T> is an extension method that casts to it internally.
        _serviceProvider = Substitute.For<IServiceProvider, IKeyedServiceProvider>();
        _keyedServiceProvider = (IKeyedServiceProvider)_serviceProvider;

        _unitOfWork.GetOrCreateRepository<Payment, int>().Returns(_paymentRepo);

        _sut = new InitiatePaymentCommandHandler(_serviceProvider, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCardPayment_ResolvesKeyedServiceAndReturnsPaymentKeys()
    {
        // Arrange
        var command = new InitiatePaymentCommand(
            Method: PaymentMethod.Card,
            AmountCents: 15000,
            Email: "student@example.com",
            FirstName: "Moaz",
            LastName: "Student",
            StudentId: Guid.NewGuid(),
            LessonId: 7
        );

        _keyedServiceProvider.GetRequiredKeyedService(typeof(IPaymentService), "card")
            .Returns(_paymentService);

        _paymentService.GetPaymentKeyAsync(command.AmountCents, command.Email, command.FirstName, command.LastName)
            .Returns(("secret-abc", "public-xyz", "paymob-order-1"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.ClientSecret.Should().Be("secret-abc");
        result.PublicKey.Should().Be("public-xyz");

        _keyedServiceProvider.Received(1).GetRequiredKeyedService(typeof(IPaymentService), "card");
    }

    [Fact]
    public async Task Handle_ValidFawryPayment_ResolvesCorrectKeyedService()
    {
        // Arrange
        var command = new InitiatePaymentCommand(
            Method: PaymentMethod.Fawry,
            AmountCents: 5000,
            Email: "student@example.com",
            FirstName: "Moaz",
            LastName: "Student",
            StudentId: Guid.NewGuid(),
            LessonId: 3
        );

        _keyedServiceProvider.GetRequiredKeyedService(typeof(IPaymentService), "fawry")
            .Returns(_paymentService);

        _paymentService.GetPaymentKeyAsync(command.AmountCents, command.Email, command.FirstName, command.LastName)
            .Returns(("secret-fawry", "public-fawry", "paymob-order-2"));

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _keyedServiceProvider.Received(1).GetRequiredKeyedService(typeof(IPaymentService), "fawry");
    }

    [Fact]
    public async Task Handle_ValidRequest_PersistsPendingPaymentWithCorrectDetails()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var command = new InitiatePaymentCommand(
            Method: PaymentMethod.Card,
            AmountCents: 20000,
            Email: "student@example.com",
            FirstName: "Moaz",
            LastName: "Student",
            StudentId: studentId,
            LessonId: 9
        );

        _keyedServiceProvider.GetRequiredKeyedService(typeof(IPaymentService), "card")
            .Returns(_paymentService);

        _paymentService.GetPaymentKeyAsync(command.AmountCents, command.Email, command.FirstName, command.LastName)
            .Returns(("secret", "public", "paymob-order-99"));

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _paymentRepo.Received(1).Add(Arg.Is<Payment>(p =>
            p.Provider == "Paymob" &&
            p.ProviderRef == "paymob-order-99" &&
            p.Amount == 200m &&
            p.Currency == "EGP" &&
            p.Status == PaymentStatus.Pending &&
            p.StudentId == studentId &&
            p.LessonId == 9));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}