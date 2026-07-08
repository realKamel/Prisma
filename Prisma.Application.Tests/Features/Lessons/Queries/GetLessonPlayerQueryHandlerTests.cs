using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Xunit;

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
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User must be authenticated to access lesson player");
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
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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
                new() { StudentId = currentUserId, ExpiresAt = DateTimeOffset.UtcNow.AddDays(10) } // متبقي 10 أيام صلاحية
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonPlayerWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        // عمل مَوك للخدمات الخارجية التي تُستدعى داخل الحلقات التكرارية (Loops)
        _videoStorageService.GetVideoUrlAsync("video-playback-123").Returns(Task.FromResult("https://streaming.com/video1"));
        _storageService.GetDownloadUrlAsync("prisma", "materials/summary.pdf").Returns(Task.FromResult("https://download.com/summary.pdf"));
        _storageService.GetDownloadUrlAsync("prisma", "assignments/task1.pdf").Returns(Task.FromResult("https://download.com/task1.pdf"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // التحقق من الحقول الأساسية وثوابت المادة والمعلم
        result.Data.Id.Should().Be(lessonId);
        result.Data.Title.Should().Be("الوحدة الأولى: قواعد");
        result.Data.Subject.Should().Be("لغه انجليزيه");
        result.Data.Teacher.Should().Be("أ. أحمد مصطفى");
        result.Data.Category.Should().Be("لغه انجليزيه · الوحدة الأولى: قواعد");
        result.Data.VideoPoster.Should().Be("poster.jpg");
        result.Data.ValidityDays.Should().BeGreaterThan(0); // التحقق من حساب الأيام المتبقية

        // التحقق من الـ Sections والـ Progress والروابط المرجعة من الـ Video Storage
        result.Data.Sections.Should().HaveCount(1);
        result.Data.Sections[0].Id.Should().Be(1); // SortOrder
        result.Data.Sections[0].SectionId.Should().Be(101);
        result.Data.Sections[0].Duration.Should().Be("00:45:00");
        result.Data.Sections[0].IsCompleted.Should().BeTrue();
        result.Data.Sections[0].Progress.Should().Be(100);
        result.Data.Sections[0].ContentUrl.Should().Be("https://streaming.com/video1");
        result.Data.Sections[0].WatchedSeconds.Should().Be(2700);

        // التحقق من الـ Materials والتحويل التلقائي لنوع الملف النصي (switch case)
        result.Data.Materials.Should().HaveCount(1);
        result.Data.Materials[0].Title.Should().Be("ملخص الدرس.pdf");
        result.Data.Materials[0].Type.Should().Be("pdf");
        result.Data.Materials[0].DownloadUrl.Should().Be("https://download.com/summary.pdf");

        // التحقق من أوبجكت الكويز
        result.Data.Quiz.Should().NotBeNull();
        result.Data.Quiz!.Id.Should().Be(5);
        result.Data.Quiz.QuestionsCount.Should().Be(2);
        result.Data.Quiz.DurationMinutes.Should().Be(20);
        result.Data.Quiz.PassingScore.Should().Be(100);
        result.Data.Quiz.IsAttempted.Should().BeTrue();

        // التحقق من أوبجكت الواجب وتاريخ الاستحقاق ورابط التحميل
        result.Data.Assignment.Should().NotBeNull();
        result.Data.Assignment!.Id.Should().Be(9);
        result.Data.Assignment.ContentURL.Should().Be("https://download.com/task1.pdf");
        result.Data.Assignment.FileName.Should().Be("حل الطالب للواجب.pdf");
        result.Data.Assignment.DueDate.Should().Be(fakeAssignment.DueDate.ToString("yyyy-MM-dd"));
    }
}