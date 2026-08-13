using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonDetailsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly GetLessonDetailsQueryHandler _sut;

    public GetLessonDetailsQueryHandlerTests()
    {
        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetLessonDetailsQueryHandler(_unitOfWork, _currentUserService, _storageService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetLessonDetailsQuery(1);
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
        var query = new GetLessonDetailsQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((LessonDetailsProjection?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain("Lesson with id '1' was not found");
    }

    [Fact]
    public async Task Handle_WhenLessonExists_ReturnsMappedLessonDetailsDto()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonDetailsQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeProjection = new LessonDetailsProjection
        {
            Id = lessonId,
            Title = "درس القراءة الأول",
            Price = 150.00m,
            Description = "شرح مفصل لدرس القراءة",
            ImageThumbnailUrl = "thumb.png",
            Outcomes = new List<string> { "أن يستخرج الطالب الأفكار العامة", "أن يثري حصيلته اللغوية" },
            PrerequisiteId = 10,
            PrerequisiteTitle = "الدرس التمهيدي",
            TeacherSubject = "اللغة العربية",
            TeacherName = "أحمد محمد",
            EnrollmentsCount = 1,
            Sections = new List<SectionProjection>
            {
                new() { Id = 101, Title = "المقدمة", Duration = TimeSpan.FromMinutes(15), IsPreview = true },
                new()
                {
                    Id = 102,
                    Title = "الشرح التفصيلي",
                    Duration = TimeSpan.FromMinutes(50),
                    IsPreview = false
                }
            }
        };

        // محاكاة جلب الدرس الأساسي (كـ projection مش entity كاملة)
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // محاكاة استعلام التحقق من اكتمال المتطلب السابق (Prerequisite) بناءً على السبيسفيكيشن الخاص به
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonPrerequisiteCompletionSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // محاكاة رابط الصورة من خدمة التخزين
        _storageService.GetDownloadUrlAsync("prisma", "thumb.png").Returns("https://cdn.prisma.com/thumb.png");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value.Id.Should().Be(lessonId);
        result.Value.Title.Should().Be("درس القراءة الأول");
        result.Value.Price.Should().Be(150.00m);
        result.Value.AboutText.Should().Be("شرح مفصل لدرس القراءة");
        result.Value.Url.Should().Be("https://cdn.prisma.com/thumb.png");
        result.Value.Subject.Should().Be("اللغة العربية");
        result.Value.Teacher.Should().Be("أحمد محمد");
        result.Value.ValidityDays.Should().Be(7);

        // التحقق من حسابات المدة الزمنية (65 دقيقة -> ساعة و 5 دقيقة)
        result.Value.Duration.Should().Be("1 ساعة و 5 دقيقة");
        result.Value.ChaptersCount.Should().Be(2);
        result.Value.StudentsCount.Should().Be(1);

        // التحقق من تفاصيل الفصول (Chapters)
        result.Value.Chapters.Should().HaveCount(2);
        result.Value.Chapters[0].Title.Should().Be("المقدمة");
        result.Value.Chapters[0].Duration.Should().Be("15 د");
        result.Value.Chapters[0].IsPreview.Should().BeTrue();

        // التحقق من النتائج التعليمية والمتطلبات السابقة
        result.Value.Outcomes.Should().Contain("أن يستخرج الطالب الأفكار العامة");
        result.Value.Prerequisites.Should().HaveCount(1);
        result.Value.Prerequisites[0].Title.Should().Be("الدرس التمهيدي");
        result.Value.Prerequisites[0].IsDone.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenLessonHasNoSectionsOrPrerequisite_ReturnsZeroDurationAndEmptyPrerequisites()
    {
        // Arrange
        var lessonId = 2;
        var query = new GetLessonDetailsQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeProjection = new LessonDetailsProjection
        {
            Id = lessonId,
            Title = "درس بدون فصول",
            TeacherName = "أحمد محمد",
            TeacherSubject = "اللغة العربية",
            Sections = new List<SectionProjection>(),
            PrerequisiteId = null
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.Should().Be("٠ دقيقة");
        result.Value.ChaptersCount.Should().Be(0);
        result.Value.Prerequisites.Should().BeEmpty();
        result.Value.Url.Should().Be(string.Empty);

        // ما فيش داعي نستدعي سبيسفيكيشن اكتمال المتطلب لو مفيش PrerequisiteId
        await _lessonRepo.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<LessonPrerequisiteCompletionSpecification>(), Arg.Any<CancellationToken>());
    }
}