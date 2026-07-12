using FluentAssertions;
using NSubstitute;
using Prisma.Application.Features.Admin.Queries.GetAdminActivitiesQuery;
using Prisma.Application.Features.AdminDashboard.Queries.GetAdminActivities;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Admin;

namespace Prisma.Application.Tests.Features.AdminDashboard.Queries;

public class GetAdminActivitiesQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Enrollment, int> _enrollmentRepo = Substitute.For<IRepository<Enrollment, int>>();
    private readonly IRepository<Payment, int> _paymentRepo = Substitute.For<IRepository<Payment, int>>();
    private readonly GetAdminActivitiesQueryHandler _sut;

    public GetAdminActivitiesQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _unitOfWork.GetOrCreateRepository<Payment, int>().Returns(_paymentRepo);

        _sut = new GetAdminActivitiesQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentsAndPaymentsExist_ReturnsMergedAndOrderedActivities()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var studentId = Guid.CreateVersion7();

        var fakeEnrollments = new List<Enrollment>
        {
            new()
            {
                Id = 1,
                StudentId = studentId,
                EnrollmentMethod = EnrollmentMethod.OnlinePayment, // تأكدي إنه مطابق للـ Enum عندك
                CreatedAt = baseTime.AddMinutes(-10),
                Lesson = new() { Title = "درس سي شارب أول" }
            }
        };

        var fakePayments = new List<Payment>
        {
            new()
            {
                Id = 100,
                StudentId = studentId,
                Amount = 150,
                Currency = "EGP",
                Provider = "Fawry",
                ProviderRef = "REF123",
                PaidAt = baseTime.AddMinutes(-5) // أحدث من الـ Enrollment
            }
        };

        _enrollmentRepo.ListAsync(Arg.Any<AdminLatestEnrollmentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeEnrollments);

        _paymentRepo.ListAsync(Arg.Any<AdminSuccessfulPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakePayments);

        // Act
        var result = await _sut.Handle(new GetAdminActivitiesQuery(), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(2);

        // الدفع أحدث فيجب أن يكون الأول
        result.Data[0].Type.Should().Be("payment");
        result.Data[0].Id.Should().Be("act-pay-100");
        result.Data[0].Details.Should().Be("150 EGP");

        // الـ Enrollment ثانياً
        result.Data[1].Type.Should().Be("enroll");
        result.Data[1].Id.Should().Be("act-enr-1");
        result.Data[1].Details.Should().Be("درس سي شارب أول");
    }

    [Fact]
    public async Task Handle_WhenMoreThanSixActivities_ReturnsOnlyTopSix()
    {
        // Arrange
        // تصحيح بناء الـ Lists هنا لمنع الـ Type Mismatch
        var fakeEnrollments = Enumerable.Range(1, 5).Select(i => new Enrollment
        {
            Id = i,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-i),
            Lesson = new() { Title = $"درس {i}" }
        }).ToList();

        var fakePayments = Enumerable.Range(1, 5).Select(i => new Payment
        {
            Id = i,
            StudentId = Guid.CreateVersion7(),
            PaidAt = DateTimeOffset.UtcNow.AddHours(-i),
            Amount = 100,
            Currency = "EGP",
            Provider = "Fawry",
            ProviderRef = $"REF{i}"
        }).ToList();

        _enrollmentRepo.ListAsync(Arg.Any<AdminLatestEnrollmentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeEnrollments);

        _paymentRepo.ListAsync(Arg.Any<AdminSuccessfulPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakePayments);

        // Act
        var result = await _sut.Handle(new GetAdminActivitiesQuery(), CancellationToken.None);

        // Assert
        result.Data.Should().HaveCount(6); // الـ Handler يكتفي بـ Take(6) فقط
    }
}