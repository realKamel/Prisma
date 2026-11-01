using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Abstractions.Services.ObjectStorage;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Teachers.Queries.GetTeacherLessonsQuery;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Tests.Features.Teacher.Queries;

public class GetTeacherLessonsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<
        IRepository<Lesson, int>
    >();

    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();

    private readonly GetTeacherLessonsQueryHandler _sut;

    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();

    public GetTeacherLessonsQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new GetTeacherLessonsQueryHandler(
            _unitOfWork,
            _currentUserService,
            _identityService,
            _storageService
        );
    }

    private static GetTeacherLessonsQuery CreateQuery() =>
        new(new PaginationParams { PageNumber = 1, PageSize = 10 });

    private Guid SetupAuthenticatedTeacher()
    {
        var teacherId = Guid.NewGuid();
        _currentUserService.UserId.Returns(teacherId);
        _identityService
            .FindByIdAsync(teacherId, Arg.Any<CancellationToken>())
            .Returns(new Prisma.Domain.Entities.UserAggregate.Teacher { Id = teacherId });

        return teacherId;
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null); // محاكاة عدم تسجيل الدخول

        // Act
        var result = await _sut.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        await _lessonRepo
            .DidNotReceive()
            .ListAsync(Arg.Any<TeacherLessonStatusesSpec>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _identityService
            .FindByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _sut.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenLessonsExist_ReturnsMappedTeacherLessonDtos()
    {
        // Arrange
        SetupAuthenticatedTeacher();
        var query = CreateQuery();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-3);

        var fakeLessons = new List<GetTeacherLessonsQueryHandler.TeacherLessonsInfo>
        {
            new(
                Id: 1,
                Name: "الكورس المكثف في النحو",
                Price: 250.00m,
                Money: Money.Create(250.00m, "EGP").Value,
                Students: 2,
                Status: LessonStatus.Active,
                ThumbnailUrl: "lessons/1/thumb.png",
                SectionsCount: 4,
                Duration: TimeDuration.FromSeconds(3600).Value,
                CompletedCount: 1,
                Revenue: 500.00m,
                CreatedAt: createdAt,
                UpdatedAt: null
            ),
            new(
                Id: 2,
                Name: "مراجعة ليلة الامتحان",
                Price: 150.00m,
                Money: Money.Create(150.00m, "EGP").Value,
                Students: 0,
                Status: LessonStatus.Drafted,
                ThumbnailUrl: null,
                SectionsCount: 0,
                Duration: TimeDuration.FromSeconds(1800).Value,
                CompletedCount: 0,
                Revenue: 0m,
                CreatedAt: createdAt,
                UpdatedAt: createdAt
            ),
        };

        _lessonRepo
            .ListAsync(
                Arg.Any<TeacherLessonsWithProjectionSpec<GetTeacherLessonsQueryHandler.TeacherLessonsInfo>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakeLessons);

        _lessonRepo
            .CountAsync(
                Arg.Any<TeacherLessonsWithProjectionSpec<GetTeacherLessonsQueryHandler.TeacherLessonsInfo>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(2);

        _lessonRepo
            .ListAsync(Arg.Any<TeacherLessonStatusesSpec>(), Arg.Any<CancellationToken>())
            .Returns(
                new List<LessonStatus>
                {
                    LessonStatus.Active,
                    LessonStatus.Drafted,
                    LessonStatus.Hidden,
                }
            );

        _storageService
            .GetDownloadUrlAsync("lessons/1/thumb.png")
            .Returns("https://cdn.example.com/lessons/1/thumb.png");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Lessons.Items.Should().HaveCount(2);
        result.Value.Lessons.TotalCount.Should().Be(2);
        result.Value.Lessons.PageNumber.Should().Be(1);
        result.Value.Lessons.PageSize.Should().Be(10);

        // فحص بيانات الدرس الأول
        var firstLesson = result.Value.Lessons.Items.First();
        firstLesson.Id.Should().Be(1);
        firstLesson.Name.Should().Be("الكورس المكثف في النحو");
        firstLesson.Price.Should().Be(250.00m);
        firstLesson.Students.Should().Be(2);
        firstLesson.Status.Should().Be(LessonStatus.Active);
        firstLesson.ThumbnailUrl.Should().Be("https://cdn.example.com/lessons/1/thumb.png");
        firstLesson.SectionsCount.Should().Be(4);
        firstLesson.DurationSeconds.Should().Be(3600);
        firstLesson.CompletionRate.Should().Be(50); // 1 من 2 = 50%
        firstLesson.Revenue.Should().Be(500.00m);
        firstLesson.CreatedAt.Should().Be(createdAt);

        // فحص بيانات الدرس الثاني
        var secondLesson = result.Value.Lessons.Items.Last();
        secondLesson.Id.Should().Be(2);
        secondLesson.Name.Should().Be("مراجعة ليلة الامتحان");
        secondLesson.Price.Should().Be(150.00m);
        secondLesson.Students.Should().Be(0);
        secondLesson.Status.Should().Be(LessonStatus.Drafted);
        secondLesson.ThumbnailUrl.Should().BeNull();
        secondLesson.CompletionRate.Should().BeNull(); // مفيش طلاب -> مفيش نسبة إكمال
        secondLesson.UpdatedAt.Should().Be(createdAt);

        // فحص الإحصائيات
        result.Value.Counts.All.Should().Be(3);
        result.Value.Counts.Active.Should().Be(1);
        result.Value.Counts.Hidden.Should().Be(1);
        result.Value.Counts.Drafted.Should().Be(1);
    }
}