using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Queries.GetTeacherCodeBatches;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.RedeemCodes;
using RedeemCodeEntity = Prisma.Domain.Entities.PaymentAggregate.RedeemCode;

namespace Prisma.Application.Tests.Features.RedeemCode;

public class GetTeacherCodeBatchesQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

    private readonly IRepository<RedeemCodeEntity, int> _batchRepo =
        Substitute.For<IRepository<RedeemCodeEntity, int>>();

    private readonly GetTeacherCodeBatchesQueryHandler _sut;

    private readonly Guid _teacherId = Guid.NewGuid();

    // private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    public GetTeacherCodeBatchesQueryHandlerTests()
    {
        _currentUser.UserId.Returns(_teacherId);
        _currentUser.IsAuthenticated.Returns(true);
        _unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>().Returns(_batchRepo);
        _sut = new GetTeacherCodeBatchesQueryHandler(_unitOfWork, _currentUser, _identityService);
    }

    [Fact]
    public async Task Handle_WhenBatchesExist_ReturnsMappedList()
    {
        // Arrange
        var fakeBatches = new List<RedeemCodeEntity>
        {
            new()
            {
                Id = 1,
                AcademicYearId = 1,
                AcademicYear = new AcademicYear { Id = 1, Title = "الأول الثانوي" },
                LessonId = 1,
                Lesson = new Domain.Entities.LessonAggregate.Lesson { Id = 1, Title = "الكهرباء الساكنة" },
                TotalCodes = 10,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                GeneratedCodes = new List<Domain.Entities.PaymentAggregate.GeneratedCode>
                {
                    new() { RedeemedByStudentId = Guid.NewGuid() }, new() { RedeemedByStudentId = null },
                },
            },
        };

        _batchRepo.ListAsync(
                Arg.Any<TeacherCodeBatchesSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(fakeBatches);

        // Act
        var result = await _sut.Handle(
            new GetTeacherCodeBatchesQuery(null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be(1);
        result.Value[0].TotalCodes.Should().Be(10);
        result.Value[0].UsedCodes.Should().Be(1); // only 1 redeemed
        result.Value[0].AcademicYear.Should().Be("الأول الثانوي");
        result.Value[0].Lesson.Should().Be("الكهرباء الساكنة");
    }

    [Fact]
    public async Task Handle_WhenNoBatches_ReturnsEmptyList()
    {
        // Arrange
        _batchRepo.ListAsync(
                Arg.Any<TeacherCodeBatchesSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<RedeemCodeEntity>());

        // Act
        var result = await _sut.Handle(
            new GetTeacherCodeBatchesQuery(null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUser.UserId.Returns((Guid?)null);
        _currentUser.IsAuthenticated.Returns(false);

        // Act
        var result = await _sut.Handle(
            new GetTeacherCodeBatchesQuery(null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }
}