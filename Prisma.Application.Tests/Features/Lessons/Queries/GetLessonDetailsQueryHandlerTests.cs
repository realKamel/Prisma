using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Xunit;

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
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetLessonDetailsQueryHandler(_unitOfWork, _currentUserService, _storageService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetLessonDetailsQuery(1);
        _currentUserService.UserId.Returns((Guid?)null); // مستخدم غير مسجل

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User is not authenticated");
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetLessonDetailsQuery(1);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // إرجاع null محاكاةً لعدم وجود الدرس
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<LessonWithDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLessonExists_ReturnsMappedLessonDetailsDto()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonDetailsQuery(lessonId);
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // بناء بيانات وهمية للدرس مع السيكشن والـ Prerequisite
        var fakePrerequisite = new Lesson { Id = 10, Title = "الدرس التمهيدي" };
        var fakeLesson = new Lesson
        {
            Id = lessonId,
            Title = "درس القراءة الأول",
            Price = 150.00m,
            Description = "شرح مفصل لدرس القراءة",
            ImageThumbnailUrl = "thumb.png",
            Outcomes = new List<string> { "أن يستخرج الطالب الأفكار العامة", "أن يثري حصيلته اللغوية" },
            Prerequisite = fakePrerequisite,
            Sections = new List<Section>
            {
                new() { Id = 101, Title = "المقدمة", Duration = TimeSpan.FromMinutes(15), IsPreview = true },
                new() { Id = 102, Title = "الشرح التفصيلي", Duration = TimeSpan.FromMinutes(50), IsPreview = false }
            },
            Enrollments = new List<Enrollment>
            {
                new() { StudentId = currentUserId, Status = EnrollmentStatus.Active }
            }
        };

        // الـ Mock للـ Repository ليرجع الدرس الأساسي
        _lessonRepo.FirstOrDefaultAsync(Arg.Is<LessonWithDetailsSpecification>(s => s != null), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        // الـ Mock الخاص بالـ Storage Service
        _storageService.GetPublicUrl("prisma", "thumb.png").Returns("https://cdn.prisma.com/thumb.png");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // التأكد من صحة البيانات المرجعة وعمل الـ Mapping
        result.Data.Id.Should().Be(lessonId);
        result.Data.Title.Should().Be("درس القراءة الأول");
        result.Data.Price.Should().Be(150.00m);
        result.Data.AboutText.Should().Be("شرح مفصل لدرس القراءة");
        result.Data.Url.Should().Be("https://cdn.prisma.com/thumb.png");

        // حسابات الوقت (15 + 50 = 65 دقيقة -> 1 ساعة و 5 دقيقة)
        result.Data.Duration.Should().Be("1 ساعة و 5 دقيقة");
        result.Data.ChaptersCount.Should().Be(2);
        result.Data.StudentsCount.Should().Be(1);

        // التأكد من تفاصيل الشباتر
        result.Data.Chapters.Should().HaveCount(2);
        result.Data.Chapters[0].Title.Should().Be("المقدمة");
        result.Data.Chapters[0].IsPreview.Should().BeTrue();

        // التأكد من الـ Outcomes والـ Prerequisites
        result.Data.Outcomes.Should().Contain("أن يستخرج الطالب الأفكار العامة");
        result.Data.Prerequisites.Should().HaveCount(1);
        result.Data.Prerequisites[0].Title.Should().Be("الدرس التمهيدي");
    }
}