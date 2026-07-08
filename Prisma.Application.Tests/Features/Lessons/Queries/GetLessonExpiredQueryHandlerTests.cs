using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonExpired;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
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
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetLessonExpiredQuery(1);
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User is not authenticated");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetLessonExpiredQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonExpiredSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLessonExists_ReturnsMappedLessonExpiredDtoWithCorrectCalculations()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonExpiredQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var expectedExpiryDate = DateTimeOffset.UtcNow.AddDays(-1); // منتهي الصلاحية كمثال

        // بناء بيانات التقدم للسكاشن (السكشن الأول %50 والثاني %100 -> المتوسط المتوقع %75)
        var sections = new List<Section>
        {
            new()
            {
                Id = 101,
                Title = "المقدمة",
                Duration = new TimeSpan(0, 30, 0), // 30 دقيقة
                Progresses = new List<SectionProgress>
                {
                    new() { StudentId = currentUserId, Percentage = 50 }
                }
            },
            new()
            {
                Id = 102,
                Title = "الدرس الرئيسي",
                Duration = new TimeSpan(1, 15, 0), // ساعة و 15 دقيقة
                Progresses = new List<SectionProgress>
                {
                    new() { StudentId = currentUserId, Percentage = 100 }
                }
            }
        };

        // بناء بيانات الكويز والدرجة
        var quizId = 5;
        var fakeQuiz = new Quiz
        {
            Id = quizId,
            Attempts = new List<QuizAttempt>
            {
                new() { QuizId = quizId, StudentId = currentUserId, Status = QuizAttemptStatus.Graded, Degree = 85.5m },
                new() { QuizId = quizId, StudentId = Guid.NewGuid(), Status = QuizAttemptStatus.Graded, Degree = 100m } // طالب آخر لا يهمنا
            }
        };

        var fakeLesson = new Lesson
        {
            Id = lessonId,
            Title = "درس الكيمياء العضوية",
            Description = "مراجعة شاملة بعد انتهاء الوقت المتاح",
            ImageThumbnailUrl = "expired-lesson.jpg",
            Price = 200.00m,
            Sections = sections,
            Quiz = fakeQuiz,
            LessonMaterials = new List<LessonMaterial> { new(), new() }, // مادتين تعليميتين
            Enrollments = new List<Enrollment>
            {
                new() { StudentId = currentUserId, ExpiresAt = expectedExpiryDate }
            }
        };

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonExpiredSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // التأكد من الحقول الأساسية وعمل الـ Mapping
        result.Data.Id.Should().Be(lessonId);
        result.Data.Title.Should().Be("درس الكيمياء العضوية");
        result.Data.Description.Should().Be("مراجعة شاملة بعد انتهاء الوقت المتاح");
        result.Data.Url.Should().Be("expired-lesson.jpg");
        result.Data.Price.Should().Be(200.00m);
        result.Data.ChaptersCount.Should().Be(2);
        result.Data.MaterialsCount.Should().Be(2);
        result.Data.ExpiredDate.Should().Be(expectedExpiryDate);

        // التأكد من الحسابات الرياضية (CalculateTotalProgress & CalculateDegree)
        result.Data.totalprogress.Should().Be(75); // (50 + 100) / 2 = 75
        result.Data.Degree.Should().Be(85.5m);

        // التأكد من الـ Chapters وصيغة الوقت الممررة (hh\:mm\:ss)
        result.Data.Chapters.Should().HaveCount(2);
        result.Data.Chapters[0].Title.Should().Be("المقدمة");
        result.Data.Chapters[0].Duration.Should().Be("00:30:00");
        result.Data.Chapters[1].Duration.Should().Be("01:15:00");
    }
}