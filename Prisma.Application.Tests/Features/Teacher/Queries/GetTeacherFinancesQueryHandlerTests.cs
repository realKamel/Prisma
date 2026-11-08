using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Teachers.Queries.GetTeacherFinancesQuery;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Tests.Features.Teacher.Queries;

public class GetTeacherFinancesQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly IRepository<Payment, int> _paymentRepo = Substitute.For<
        IRepository<Payment, int>
    >();

    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();

    private readonly GetTeacherFinancesQueryHandler _sut;

    public GetTeacherFinancesQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Payment, int>().Returns(_paymentRepo);
        _sut = new GetTeacherFinancesQueryHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsUnauthorizedResult()
    {
        // Arrange
        var query = new GetTeacherFinancesQuery();
        _currentUserService.UserId.Returns((Guid?)null); // محاكاة عدم تسجيل الدخول

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        await _paymentRepo
            .DidNotReceive()
            .ListAsync(
                Arg.Any<TeacherFinancesSpecification<Financesinfo>>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenPaymentsExist_ReturnsMappedRawTransactionDtos()
    {
        // Arrange
        var query = new GetTeacherFinancesQuery();
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var paidAt = new DateTimeOffset(2026, 5, 20, 10, 0, 0, TimeSpan.Zero);
        var fakePayments = new List<Financesinfo>
        {
            new(
                Id: 1001,
                Amount: Money.Create(150.00m).Value,
                PaidAt: paidAt,
                StudentFirstName: "احمد",
                StudentLastName: "علي",
                LessonTitle: "اللغة الإنجليزية - الدرس الأول"
            ),
        };

        _paymentRepo
            .ListAsync(
                Arg.Any<TeacherFinancesSpecification<Financesinfo>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakePayments);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var transaction = result.Value[0];
        transaction.Id.Should().Be("1001");
        transaction.StudentName.Should().Be("احمد علي");
        transaction.LessonTitle.Should().Be("اللغة الإنجليزية - الدرس الأول");
        transaction.Money.Amount.Should().Be(150.00m);
        transaction.Money.Currency.Should().Be("EGP");
        transaction.Date.Should().Be(paidAt);
    }

    [Fact]
    public async Task Handle_WhenPaymentDetailsAreNull_ReturnsFallbackValues()
    {
        // Arrange
        var query = new GetTeacherFinancesQuery();
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var fakePaymentsWithNulls = new List<Financesinfo>
        {
            new(
                Id: 1002,
                Amount: Money.Create(200.00m).Value,
                PaidAt: null,
                StudentFirstName: null,
                StudentLastName: null,
                LessonTitle: null
            ),
        };

        _paymentRepo
            .ListAsync(
                Arg.Any<TeacherFinancesSpecification<Financesinfo>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakePaymentsWithNulls);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var transaction = result.Value[0];
        transaction.Id.Should().Be("1002");
        transaction.StudentName.Should().Be("طالب غير معروف");
        transaction.LessonTitle.Should().Be("درس غير معروف");
        transaction.Money.Amount.Should().Be(200.00m);
        transaction.Date.Should().BeNull();
    }
}
