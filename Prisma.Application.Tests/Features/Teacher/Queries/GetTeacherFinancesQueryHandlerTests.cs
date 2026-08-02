using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Teachers.Queries.GetTeacherFinances;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Prisma.Application.Features.Teachers.Queries.GetTeacherFinancesQuery;
using Prisma.Domain.Specifications.Teacher;
using Xunit;

namespace Prisma.Application.Tests.Features.Teachers.Queries;

public class GetTeacherFinancesQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Payment, int> _paymentRepo = Substitute.For<IRepository<Payment, int>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
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
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenPaymentsExist_ReturnsMappedRawTransactionDtos()
    {
        // Arrange
        var query = new GetTeacherFinancesQuery();
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var fakePayments = new List<Payment>
        {
            new()
            {
                Id = 1001,
                Amount = 150.00m,
                PaidAt = new DateTimeOffset(2026, 5, 20, 10, 0, 0, TimeSpan.Zero),
                // تمرير نصوص عادية للتوافق التام مع نتيجة المقارنة الفعليّة للـ Handler
                Student = new Student { FirstName = "احمد", LastName = "علي" },
                Lesson = new Lesson { Id = 1, Title = "اللغة الإنجليزية - الدرس الأول" }
            }
        };

        _paymentRepo.ListAsync(Arg.Any<TeacherFinancesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakePayments);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.HaveCount(1);

        var transaction = result.Value[0];
        transaction.Id.Should().Be("1001");

        // التعديل النهائي الصحيح: مطابقة مع القيمة الحقيقية الخارجة من الـ Handler تماماً
        transaction.StudentName.Should().Be("احمد علي");

        transaction.LessonTitle.Should().Be("اللغة الإنجليزية - الدرس الأول");
        transaction.Amount.Should().Be(150.00m);
        transaction.Date.Should().Be("2026-05-20");
    }

    [Fact]
    public async Task Handle_WhenPaymentDetailsAreNull_ReturnsFallbackValues()
    {
        // Arrange
        var query = new GetTeacherFinancesQuery();
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var fakePaymentsWithNulls = new List<Payment>
        {
            new()
            {
                Id = 1002,
                Amount = 200.00m,
                PaidAt = null,
                Student = null,
                Lesson = null
            }
        };

        _paymentRepo.ListAsync(Arg.Any<TeacherFinancesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakePaymentsWithNulls);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.HaveCount(1);

        var transaction = result.Value[0];
        transaction.Id.Should().Be("1002");
        transaction.StudentName.Should().Be("طالب غير معروف");
        transaction.LessonTitle.Should().Be("درس غير معروف");
        transaction.Amount.Should().Be(200.00m);
        transaction.Date.Should().Be(string.Empty);
    }
}