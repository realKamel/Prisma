using FluentAssertions;
using NSubstitute;
using Prisma.Application.Features.Admin.Queries.GetAdminActivitiesQuery;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Admin;

namespace Prisma.Application.Tests.Features.Admin.Queries;

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

        var fakeEnrollments = new List<EnrollmentActivityProjection>
        {
            new EnrollmentActivityProjection(
                1,
                studentId,
                "درس سي شارب أول",
                EnrollmentMethod.OnlinePayment, // تأكدي إنه مطابق للـ Enum عندك
                baseTime.AddMinutes(-10)
            )
        };

        var fakePayments = new List<PaymentActivityProjection>
        {
            new PaymentActivityProjection(
                100,
                studentId,
                150,
                "EGP",
                "Fawry",
                "REF123",
                baseTime.AddMinutes(-5),
                baseTime.AddMinutes(-5)
            )
        };

        _enrollmentRepo.ListAsync(Arg.Any<AdminLatestEnrollmentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeEnrollments);

        _paymentRepo.ListAsync(Arg.Any<AdminSuccessfulPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakePayments);

        // Act
        var result = await _sut.Handle(new GetAdminActivitiesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        // الدفع أحدث فيجب أن يكون الأول
        result.Value[0].Type.Should().Be("payment");
        result.Value[0].Id.Should().Be("act-pay-100");
        result.Value[0].Details.Should().Be("150 EGP");

        // الـ Enrollment ثانياً
        result.Value[1].Type.Should().Be("enroll");
        result.Value[1].Id.Should().Be("act-enr-1");
        result.Value[1].Details.Should().Be("درس سي شارب أول");
    }

    [Fact]
    public async Task Handle_WhenMoreThanSixActivities_ReturnsOnlyTopSix()
    {
        // Arrange
        // تصحيح بناء الـ Lists هنا لمنع الـ Type Mismatch
        var fakeEnrollments = Enumerable.Range(1, 5).Select(i => new EnrollmentActivityProjection
        (
            i,
            Guid.CreateVersion7(),
            $"درس {i}",
            EnrollmentMethod.OnlinePayment,
            DateTimeOffset.UtcNow.AddHours(-i)
        )).ToList();

        var fakePayments = Enumerable.Range(1, 5).Select(i => new PaymentActivityProjection
        (
            i,
            Guid.CreateVersion7(),
            100,
            "EGP",
            "Fawry",
            $"REF{i}",
            DateTimeOffset.UtcNow.AddHours(-i),
            DateTimeOffset.UtcNow.AddHours(-i)
        )).ToList();

        _enrollmentRepo.ListAsync(Arg.Any<AdminLatestEnrollmentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeEnrollments);

        _paymentRepo.ListAsync(Arg.Any<AdminSuccessfulPaymentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakePayments);

        // Act
        var result = await _sut.Handle(new GetAdminActivitiesQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(6); // الـ Handler يكتفي بـ Take(6) فقط
    }
}