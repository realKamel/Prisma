using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Lessons.Queries.GetLessonExpired;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonExpiredQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetLessonExpiredQueryHandler _sut;

    public GetLessonExpiredQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetLessonExpiredQueryHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetLessonExpiredQuery(1);
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User is not authenticated");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var query = new GetLessonExpiredQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonExpiredSpecification>(), Arg.Any<CancellationToken>())
            .Returns((LessonExpiredProjection?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenLessonExists_ReturnsMappedLessonExpiredDto()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonExpiredQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var expectedExpiryDate = DateTimeOffset.UtcNow.AddDays(-1); // منتهي الصلاحية كمثال

        // NOTE: التقدم (progress) ودرجة الكويز بقوا محسوبين جوه الـ Specification نفسها
        // (LessonExpiredProjection.TotalProgress / Degree)، فالتيست بيحط القيم الناتجة
        // مباشرة بدل ما يبني Sections/Progresses/QuizAttempts كاملة.
        var fakeProjection = new LessonExpiredProjection
        {
            Id = lessonId,
            Title = "درس الكيمياء العضوية",
            Description = "مراجعة شاملة بعد انتهاء الوقت المتاح",
            ImageThumbnailUrl = "expired-lesson.jpg",
            Price = 200.00m,
            ChaptersCount = 2,
            MaterialsCount = 2,
            ExpiredDate = expectedExpiryDate,
            TotalProgress = 75, // (50 + 100) / 2 = 75
            Degree = 85.5m,
            Chapters = new List<ExpiredChapterProjection>
            {
                new() { Id = 101, Title = "المقدمة", Duration = new TimeSpan(0, 30, 0) },
                new() { Id = 102, Title = "الدرس الرئيسي", Duration = new TimeSpan(1, 15, 0) }
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonExpiredSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // التأكد من الحقول الأساسية وعمل الـ Mapping
        result.Value.Id.Should().Be(lessonId);
        result.Value.Title.Should().Be("درس الكيمياء العضوية");
        result.Value.Description.Should().Be("مراجعة شاملة بعد انتهاء الوقت المتاح");
        result.Value.Url.Should().Be("expired-lesson.jpg");
        result.Value.Price.Should().Be(200.00m);
        result.Value.ChaptersCount.Should().Be(2);
        result.Value.MaterialsCount.Should().Be(2);
        result.Value.ExpiredDate.Should().Be(expectedExpiryDate);

        result.Value.totalprogress.Should().Be(75);
        result.Value.Degree.Should().Be(85.5m);

        // التأكد من الـ Chapters وصيغة الوقت الممررة (hh\:mm\:ss)
        result.Value.Chapters.Should().HaveCount(2);
        result.Value.Chapters[0].Title.Should().Be("المقدمة");
        result.Value.Chapters[0].Duration.Should().Be("00:30:00");
        result.Value.Chapters[1].Duration.Should().Be("01:15:00");
    }
}