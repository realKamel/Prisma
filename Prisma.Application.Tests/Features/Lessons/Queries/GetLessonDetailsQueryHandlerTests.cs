using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.ValueObjects.ContentDomain;
using Prisma.Domain.ValueObjects.EnrollmentDomain;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonDetailsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<
        IRepository<Lesson, int>
    >();
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly GetLessonDetailsQueryHandler _sut;

    public GetLessonDetailsQueryHandlerTests()
    {
        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetLessonDetailsQueryHandler(_unitOfWork, _currentUserService, _storageService);
    }

    private static LessonDetailsProjection CreateProjection(
        Guid lessonId,
        List<SectionProjection>? sections = null,
        int? prerequisiteId = null,
        string? prerequisiteTitle = null
    ) =>
        new()
        {
            Id = lessonId,
            Title = "درس القراءة الأول",
            Price = 150.00m,
            Money = Money.Create(150.00m, "EGP").Value,
            Description = "شرح مفصل لدرس القراءة",
            ImageThumbnailUrl = "thumb.png",
            EnrollmentsCount = 1,
            TeacherName = "أحمد محمد",
            TeacherSubject = "اللغة العربية",
            ValidityDuration = TimeDuration.FromSeconds(7 * 24 * 60 * 60).Value,
            ValidityRange = DateRange
                .Create(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(7 * 24 * 60 * 60))
                .Value,
            Outcomes = new List<string>
            {
                "أن يستخرج الطالب الأفكار العامة",
                "أن يثري حصيلته اللغوية",
            },
            Sections = sections ?? [],
            PrerequisiteId = prerequisiteId,
            PrerequisiteTitle = prerequisiteTitle,
        };

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetLessonDetailsQuery(Guid.NewGuid());
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        await _lessonRepo
            .DidNotReceive()
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var query = new GetLessonDetailsQuery(lessonId);
        _currentUserService.UserId.Returns(Guid.NewGuid());

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((LessonDetailsProjection?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Lesson with id '{lessonId}' was not found");
    }

    [Fact]
    public async Task Handle_WhenLessonExists_ReturnsMappedLessonDetailsDto()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var query = new GetLessonDetailsQuery(lessonId);
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var fakeProjection = CreateProjection(
            lessonId,
            sections:
            [
                new()
                {
                    Id = 101,
                    Title = "المقدمة",
                    Duration = TimeDuration.FromMinutes(15).Value,
                    IsPreview = true,
                },
                new()
                {
                    Id = 102,
                    Title = "الشرح التفصيلي",
                    Duration = TimeDuration.FromMinutes(50).Value,
                    IsPreview = false,
                },
            ],
            prerequisiteId: 10,
            prerequisiteTitle: "الدرس التمهيدي"
        );

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakeProjection);

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonPrerequisiteCompletionSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(true);

        _storageService
            .GetDownloadUrlAsync("prisma", "thumb.png")
            .Returns("https://cdn.prisma.com/thumb.png");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(lessonId);
        result.Value.Title.Should().Be("درس القراءة الأول");
        result.Value.Money.Amount.Should().Be(150.00m);
        result.Value.Money.Currency.Should().Be("EGP");
        result.Value.AboutText.Should().Be("شرح مفصل لدرس القراءة");
        result.Value.Url.Should().Be("https://cdn.prisma.com/thumb.png");
        result.Value.Subject.Should().Be("اللغة العربية");
        result.Value.Teacher.Should().Be("أحمد محمد");
        result.Value.ValidityDuration.Seconds.Should().Be(7 * 24 * 60 * 60);
        result.Value.ValidityDateRange.Should().NotBeNull();
        result.Value.ValidityDateRange!.Duration.Seconds.Should().Be(7 * 24 * 60 * 60);

        // 15 + 50 دقيقة
        result.Value.Duration.Seconds.Should().Be((15 + 50) * 60);
        result.Value.ChaptersCount.Should().Be(2);
        result.Value.StudentsCount.Should().Be(1);

        result.Value.Chapters.Should().HaveCount(2);
        result.Value.Chapters[0].Id.Should().Be(101);
        result.Value.Chapters[0].Title.Should().Be("المقدمة");
        result.Value.Chapters[0].Duration.Seconds.Should().Be(15 * 60);
        result.Value.Chapters[0].IsPreview.Should().BeTrue();
        result.Value.Chapters[1].Id.Should().Be(102);
        result.Value.Chapters[1].IsPreview.Should().BeFalse();

        result.Value.Outcomes.Should().Contain("أن يستخرج الطالب الأفكار العامة");
        result.Value.Prerequisites.Should().HaveCount(1);
        result.Value.Prerequisites[0].Title.Should().Be("الدرس التمهيدي");
        result.Value.Prerequisites[0].IsDone.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenLessonHasNoSectionsOrPrerequisite_ReturnsZeroDurationAndEmptyPrerequisites()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var query = new GetLessonDetailsQuery(lessonId);
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var fakeProjection = CreateProjection(lessonId, sections: []);

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakeProjection);

        _storageService
            .GetDownloadUrlAsync("prisma", "thumb.png")
            .Returns("https://cdn.prisma.com/thumb.png");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.Seconds.Should().Be(0);
        result.Value.ChaptersCount.Should().Be(0);
        result.Value.Chapters.Should().BeEmpty();
        result.Value.Prerequisites.Should().BeEmpty();

        // ما فيش داعي نستدعي سبيسفيكيشن اكتمال المتطلب لو مفيش PrerequisiteId
        await _lessonRepo
            .DidNotReceive()
            .FirstOrDefaultAsync(
                Arg.Any<LessonPrerequisiteCompletionSpecification>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenLessonHasNoThumbnail_ReturnsEmptyUrlWithoutCallingStorage()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var query = new GetLessonDetailsQuery(lessonId);
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var fakeProjection = CreateProjection(lessonId) with { ImageThumbnailUrl = null };

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<LessonWithDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().BeEmpty();

        await _storageService
            .DidNotReceive()
            .GetDownloadUrlAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }
}
