using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonPlayerQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IVideoStorageService _videoStorageService = Substitute.For<IVideoStorageService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly GetLessonPlayerQueryHandler _sut;

    public GetLessonPlayerQueryHandlerTests()
    {
        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetLessonPlayerQueryHandler(_unitOfWork, _currentUserService, _videoStorageService, _storageService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var query = new GetLessonPlayerQuery(1);
        _currentUserService.UserId.Returns((Guid?)null); // مستخدم غير مسجل دخول

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User must be authenticated to access lesson player");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var query = new GetLessonPlayerQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // محاكاة إرجاع null عند البحث عن الدرس
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonPlayerWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((LessonPlayerProjection?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenLessonExists_ReturnsCompleteMappedLessonPlayerResult()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonPlayerQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // 1. إعداد بيانات الـ Sections والـ Progress (كـ projection مباشرة، الحساب حصل جوه السبيسفيكيشن)
        var fakeSections = new List<PlayerSectionProjection>
        {
            new()
            {
                Id = 101,
                SortOrder = 1,
                Title = "شرح قاعدة المضارع البسيط",
                Duration = new TimeSpan(0, 45, 0),
                PlaybackId = "video-playback-123",
                IsCompleted = true,
                WatchedSeconds = 2700
            }
        };

        // 2. إعداد بيانات المواد الدراسية الـ Materials (Type رقم مباشرة زي ما بترجعه الـ projection)
        var fakeMaterials = new List<PlayerMaterialProjection>
        {
            new()
            {
                Title = "ملخص الدرس.pdf",
                DownloadUrl = "materials/summary.pdf",
                Type = 0 // الهاندلر بيحولها لـ "pdf"
            }
        };

        // 3. إعداد الكويز
        var fakeQuiz = new PlayerQuizProjection
        {
            Id = 5,
            QuestionsCount = 2,
            TimeInMinutes = TimeSpan.FromMinutes(20),
            TotalDegree = 100,
            IsAttempted = true
        };

        // 4. إعداد الواجب
        var fakeAssignment = new PlayerAssignmentProjection
        {
            Id = 9,
            ContentURL = "assignments/task1.pdf",
            DueDate = DateTimeOffset.UtcNow.AddDays(5),
            SubmissionTitle = "حل الطالب للواجب.pdf"
        };

        // 5. تجميع بيانات الدرس (projection كامل بدل Lesson entity)
        var fakeProjection = new LessonPlayerProjection
        {
            Id = lessonId,
            Title = "الوحدة الأولى: قواعد",
            Description = "شرح تفصيلي لقواعد الوحدة الأولى",
            ImageThumbnailUrl = "poster.jpg",
            Subject = "لغه انجليزيه",
            TeacherName = "أ. أحمد مصطفى",
            EnrollmentExpiresAt = DateTimeOffset.UtcNow.AddDays(10), // متبقي 10 أيام صلاحية
            Sections = fakeSections,
            Materials = fakeMaterials,
            Quiz = fakeQuiz,
            Assignment = fakeAssignment
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonPlayerWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // عمل مَوك للخدمات الخارجية التي تُستدعى داخل الحلقات التكرارية (Loops)
        _videoStorageService.GetVideoUrlAsync("video-playback-123")
            .Returns(Task.FromResult("https://streaming.com/video1"));
        _storageService.GetDownloadUrlAsync("prisma", "materials/summary.pdf")
            .Returns(Task.FromResult("https://download.com/summary.pdf"));
        _storageService.GetDownloadUrlAsync("prisma", "assignments/task1.pdf")
            .Returns(Task.FromResult("https://download.com/task1.pdf"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // التحقق من الحقول الأساسية وثوابت المادة والمعلم
        result.Value.Id.Should().Be(lessonId);
        result.Value.Title.Should().Be("الوحدة الأولى: قواعد");
        result.Value.Subject.Should().Be("لغه انجليزيه");
        result.Value.Teacher.Should().Be("أ. أحمد مصطفى");
        result.Value.Category.Should().Be("لغه انجليزيه · الوحدة الأولى: قواعد");
        result.Value.VideoPoster.Should().Be("poster.jpg");
        result.Value.ValidityDays.Should().BeGreaterThan(0); // التحقق من حساب الأيام المتبقية

        // التحقق من الـ Sections والـ Progress والروابط المرجعة من الـ Video Storage
        result.Value.Sections.Should().HaveCount(1);
        result.Value.Sections[0].Id.Should().Be(1); // SortOrder
        result.Value.Sections[0].SectionId.Should().Be(101);
        result.Value.Sections[0].Duration.Should().Be("00:45:00");
        result.Value.Sections[0].IsCompleted.Should().BeTrue();
        result.Value.Sections[0].Progress.Should().Be(100);
        result.Value.Sections[0].ContentUrl.Should().Be("https://streaming.com/video1");
        result.Value.Sections[0].WatchedSeconds.Should().Be(2700);

        // التحقق من الـ Materials والتحويل التلقائي لنوع الملف من الرقم للنص (switch case)
        result.Value.Materials.Should().HaveCount(1);
        result.Value.Materials[0].Title.Should().Be("ملخص الدرس.pdf");
        result.Value.Materials[0].Type.Should().Be("pdf");
        result.Value.Materials[0].DownloadUrl.Should().Be("https://download.com/summary.pdf");

        // التحقق من أوبجكت الكويز
        result.Value.Quiz.Should().NotBeNull();
        result.Value.Quiz!.Id.Should().Be(5);
        result.Value.Quiz.QuestionsCount.Should().Be(2);
        result.Value.Quiz.DurationMinutes.Should().Be(20);
        result.Value.Quiz.PassingScore.Should().Be(100);
        result.Value.Quiz.IsAttempted.Should().BeTrue();

        // التحقق من أوبجكت الواجب وتاريخ الاستحقاق ورابط التحميل
        result.Value.Assignment.Should().NotBeNull();
        result.Value.Assignment!.Id.Should().Be(9);
        result.Value.Assignment.ContentURL.Should().Be("https://download.com/task1.pdf");
        result.Value.Assignment.FileName.Should().Be("حل الطالب للواجب.pdf");
        result.Value.Assignment.DueDate.Should().Be(fakeAssignment.DueDate.ToString("yyyy-MM-dd"));
    }

    [Fact]
    public async Task Handle_WhenEnrollmentExpiresAtIsNullOrInPast_FallsBackToThirtyDayValidity()
    {
        // Arrange
        var lessonId = 2;
        var query = new GetLessonPlayerQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeProjection = new LessonPlayerProjection
        {
            Id = lessonId,
            Title = "درس بدون اشتراك فعال",
            Subject = "لغه انجليزيه",
            TeacherName = "أ. أحمد مصطفى",
            EnrollmentExpiresAt = null,
            Sections = new List<PlayerSectionProjection>(),
            Materials = new List<PlayerMaterialProjection>()
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonPlayerWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ValidityDays.Should().Be(30);
        result.Value.Quiz.Should().BeNull();
        result.Value.Assignment.Should().BeNull();
    }
}