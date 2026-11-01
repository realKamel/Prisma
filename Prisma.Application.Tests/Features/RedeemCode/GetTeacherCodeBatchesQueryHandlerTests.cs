using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.RedeemCodes.Queries.GetTeacherCodeBatches;
using Prisma.Domain.Entities.UserAggregate;
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

    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    public GetTeacherCodeBatchesQueryHandlerTests()
    {
        _currentUser.UserId.Returns(_teacherId);
        _currentUser.IsAuthenticated.Returns(true);
        _identityService
            .FindByIdAsync(_teacherId, Arg.Any<CancellationToken>())
            .Returns(new Prisma.Domain.Entities.UserAggregate.Teacher { Id = _teacherId });
        _unitOfWork.GetOrCreateRepository<RedeemCodeEntity, int>().Returns(_batchRepo);
        _sut = new GetTeacherCodeBatchesQueryHandler(_unitOfWork, _currentUser, _identityService);
    }

    [Fact]
    public async Task Handle_WhenBatchesExist_ReturnsMappedList()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var fakeBatches = new List<CodeBatchinfo>
        {
            new(
                Id: 1,
                AcademicYearId: 1,
                AcademicYear: "الأول الثانوي",
                LessonId: 1,
                Lesson: "الكهرباء الساكنة",
                CreatedAt: createdAt,
                TotalCodes: 10,
                UsedCodes: 1
            ),
        };

        _batchRepo.ListAsync(
                Arg.Any<TeacherCodeBatchesSpecification<CodeBatchinfo>>(),
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
        result.Value[0].CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_WhenNoBatches_ReturnsEmptyList()
    {
        // Arrange
        _batchRepo.ListAsync(
                Arg.Any<TeacherCodeBatchesSpecification<CodeBatchinfo>>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<CodeBatchinfo>());

        // Act
        var result = await _sut.Handle(
            new GetTeacherCodeBatchesQuery(null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsUnauthorized()
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

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        _identityService
            .FindByIdAsync(_teacherId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.Handle(
            new GetTeacherCodeBatchesQuery(null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}