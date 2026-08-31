using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Teachers.Queries.GetTeacherLessonsQuery;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

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

    public GetTeacherLessonsQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _sut = new GetTeacherLessonsQueryHandler(_unitOfWork, _currentUserService, _identityService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetTeacherLessonsQuery();
        _currentUserService.UserId.Returns((Guid?)null); // محاكاة عدم تسجيل الدخول

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenLessonsExist_ReturnsMappedTeacherLessonDtos()
    {
        // Arrange
        var query = new GetTeacherLessonsQuery();
        _currentUserService.UserId.Returns(Guid.NewGuid());

        var fakeLessons = new List<Lesson>
        {
            new()
            {
                Id = 1,
                Title = "الكورس المكثف في النحو",
                Price = 250.00m,
                Status = LessonStatus.Active, // يتأكد الـ Test من تحويلها لـ "active" حروف صغيرة
                Enrollments =
                    new List<Enrollment>
                    {
                        new() { Id = 101, StudentId = Guid.NewGuid() },
                        new() { Id = 102, StudentId = Guid.NewGuid() },
                    },
            },
            new()
            {
                Id = 2,
                Title = "مراجعة ليلة الامتحان",
                Price = 150.00m,
                Status = LessonStatus.Drafted, // ستتحول لـ "draft" حروف صغيرة
                Enrollments = null, // لاختبار الـ null coalescing وإرجاع 0 طلاب
            },
        };

        _lessonRepo
            .ListAsync(Arg.Any<TeacherLessonsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLessons);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.HaveCount(2);

        // فحص بيانات الدرس الأول
        var firstLesson = result.Value[0];
        firstLesson.Id.Should().Be(1);
        firstLesson.Name.Should().Be("الكورس المكثف في النحو");
        firstLesson.Price.Should().Be(250.00m);
        firstLesson.Students.Should().Be(2);
        firstLesson.Status.Should().Be("active");

        // فحص بيانات الدرس الثاني
        var secondLesson = result.Value[1];
        secondLesson.Id.Should().Be(2);
        secondLesson.Name.Should().Be("مراجعة ليلة الامتحان");
        secondLesson.Price.Should().Be(150.00m);
        secondLesson.Students.Should().Be(0); // التأكد من معالجة الـ null بشكل سليم
        secondLesson.Status.Should().Be("drafted");
    }
}