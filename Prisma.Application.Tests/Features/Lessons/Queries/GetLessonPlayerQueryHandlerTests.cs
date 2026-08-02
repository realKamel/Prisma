using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
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
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
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
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetLessonPlayerQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // محاكاة إرجاع null عند البحث عن الدرس
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonPlayerWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson)null);

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

        // 1. إعداد بيانات الـ Sections والـ Progress
        var fakeSections = new List<Section>
        {
            new()
            {
                Id = 101,
                Title = "شرح قاعدة المضارع البسيط",
                Duration = new TimeSpan(0, 45, 0),
                SortOrder = 1,
                PlaybackId = "video-playback-123",
                Progresses = new List<SectionProgress>
                {
                    new() { StudentId = currentUserId, IsCompleted = true, WatchedSeconds = 2700 }
                }
            }
        };

        // 2. إعداد بيانات المواد الدراسية الـ Materials
        var fakeMaterials = new List<LessonMaterial>
        {
            new()
            {
                Id = 201,
                Title = "ملخص الدرس.pdf",
                DownloadUrl = "materials/summary.pdf",
                Type = 0 // التعديل هنا: مرر الرقم 0 مباشرةً ليقوم الـ Handler بتحويله إلى "pdf"
            }
        };

        // 3. إعداد الكويز ومحاولة الطالب
        var fakeQuiz = new Quiz
        {
            Id = 5,
            TimeInMinutes = TimeSpan.FromMinutes(20),
            TotalDegree = 100,
            Questions = new List<QuestionLessonQuiz> { new(), new() }, // سؤالين
            Attempts = new List<QuizAttempt>
            {
                new() { StudentId = currentUserId } // الطالب قام بمحاولة حل الكويز سابقاً
            }
        };

        // 4. إعداد الواجب وتطبيق الإرسال
        var fakeAssignment = new Assignment
        {
            Id = 9,
            ContentURL = "assignments/task1.pdf",
            DueDate = DateTimeOffset.UtcNow.AddDays(5),
            Submissions = new List<AssignmentSubmission>
            {
                new() { StudentId = currentUserId, Title = "حل الطالب للواجب.pdf" }
            }
        };

        // 5. تجميع بيانات الدرس والـ Enrollment
        var fakeLesson = new Lesson
        {
            Id = lessonId,
            Title = "الوحدة الأولى: قواعد",
            Description = "شرح تفصيلي لقواعد الوحدة الأولى",
            ImageThumbnailUrl = "poster.jpg",
            Price = 150.00m,
            Sections = fakeSections,
            LessonMaterials = fakeMaterials,
            Quiz = fakeQuiz,
            Assignment = fakeAssignment,
            Enrollments = new List<Enrollment>
            {
                new()
                {
                    StudentId = currentUserId, ExpiresAt = DateTimeOffset.UtcNow.AddDays(10)
                } // متبقي 10 أيام صلاحية
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonPlayerWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

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

        // التحقق من الـ Materials والتحويل التلقائي لنوع الملف النصي (switch case)
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
}