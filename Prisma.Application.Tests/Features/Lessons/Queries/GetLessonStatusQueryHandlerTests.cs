using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Lessons.Queries.GetLessonStatus;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonStatusQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetLessonStatusQueryHandler _sut;

    public GetLessonStatusQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new GetLessonStatusQueryHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetLessonStatusQuery(1);
        _currentUserService.UserId.Returns((Guid?)null); // غير مسجل دخول

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var query = new GetLessonStatusQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns((LessonStatusProjection?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNoEnrollmentExists_ReturnsAvailableStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeProjection = new LessonStatusProjection
        {
            Id = lessonId,
            HasEnrollment = false // الطالب لم يشتريه بعد
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LessonCatalogStatus.Available);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentIsExpired_ReturnsExpiredStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeProjection = new LessonStatusProjection
        {
            Id = lessonId,
            HasEnrollment = true,
            EnrollmentExpiresAt = DateTimeOffset.UtcNow.AddDays(-1) // منتهي الصلاحية منذ يوم
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LessonCatalogStatus.Expired);
    }

    [Fact]
    public async Task Handle_WhenPrerequisiteIsNotCompleted_ReturnsLockedStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeProjection = new LessonStatusProjection
        {
            Id = lessonId,
            HasEnrollment = true,
            EnrollmentExpiresAt = DateTimeOffset.UtcNow.AddDays(5), // مسجل وصالح
            HasPrerequisite = true,
            IsPrerequisiteCompleted = false // لم يكتمل بعد!
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LessonCatalogStatus.Locked);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentIsValidAndPrerequisitesMet_ReturnsPurchasedStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeProjection = new LessonStatusProjection
        {
            Id = lessonId,
            HasEnrollment = true,
            EnrollmentExpiresAt = DateTimeOffset.UtcNow.AddDays(5), // التسجيل صالح وممتد
            HasPrerequisite = true,
            IsPrerequisiteCompleted = true // مكتمل بنجاح
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LessonCatalogStatus.Purchased);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentValidAndNoPrerequisite_ReturnsPurchasedStatus()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonStatusQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeProjection = new LessonStatusProjection
        {
            Id = lessonId,
            HasEnrollment = true,
            EnrollmentExpiresAt = DateTimeOffset.UtcNow.AddDays(5),
            HasPrerequisite = false // مفيش متطلب سابق أصلاً
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonStatusSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(LessonCatalogStatus.Purchased);
    }
}