using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Students.Queries.GetLessonsCatalog;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Tests.Features.LessonCatalog.Queries;

public class GetLessonsCatalogQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IRepository<Student, Guid> _studentRepository = Substitute.For<
        IRepository<Student, Guid>
    >();
    private readonly IRepository<Lesson, int> _lessonRepository = Substitute.For<
        IRepository<Lesson, int>
    >();
    private readonly GetLessonsCatalogQueryHandler _handler;

    private static readonly GetLessonsCatalogQuery ValidQuery = new();
    private static readonly Guid StudentId = Guid.NewGuid();
    private const int AcademicYearId = 1;

    public GetLessonsCatalogQueryHandlerTests()
    {
        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepository);
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepository);

        _handler = new GetLessonsCatalogQueryHandler(_unitOfWork, _currentUser, _storageService);
    }

    #region Helpers

    private static Domain.Entities.UserAggregate.Teacher CreateTeacher(
        string firstName = "Ahmed",
        string lastName = "Mostafa",
        string subject = "Math"
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Subject = subject,
        };

    private static Student CreateStudent(
        int? academicYearId,
        params Domain.Entities.UserAggregate.Teacher[] teachers
    )
    {
        var student = new Student
        {
            Id = StudentId,
            FirstName = "Sara",
            LastName = "Ali",
            AcademicYearId = academicYearId,
        };

        foreach (var teacher in teachers)
        {
            student.TeacherStudents.Add(
                new TeacherStudent { StudentId = student.Id, TeacherId = teacher.Id }
            );
        }

        return student;
    }

    private static Lesson CreateLesson(
        int id,
        decimal price = 100m,
        string? thumbnailUrl = null,
        int? prerequisiteId = null,
        TimeSpan? duration = null
    )
    {
        var lesson = new Lesson
        {
            Id = id,
            Title = $"Lesson {id}",
            Price = price,
            Duration = duration ?? TimeSpan.FromHours(2),
            ImageThumbnailUrl = thumbnailUrl,
            PrerequisiteId = prerequisiteId,
        };
        return lesson;
    }

    private static Enrollment CreateEnrollment(
        Guid studentId,
        DateTimeOffset? expiresAt = null,
        bool isCompleted = false
    ) =>
        new()
        {
            StudentId = studentId,
            ExpiresAt = expiresAt,
            IsCompleted = isCompleted,
        };

    private void SetupStudentAndLessons(Student student, ICollection<Lesson> lessons)
    {
        _currentUser.UserId.Returns(StudentId);
        _studentRepository.GetByIdAsync(StudentId, Arg.Any<CancellationToken>()).Returns(student);
        _lessonRepository
            .ListAsync(Arg.Any<LessonsCatalogSpecification>(), Arg.Any<CancellationToken>())
            .Returns((List<Lesson>)lessons);
    }

    #endregion

    #region Guards

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUser.UserId.Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);

        await _studentRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStudentNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUser.UserId.Returns(StudentId);
        _studentRepository
            .GetByIdAsync(StudentId, Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);

        await _lessonRepository
            .DidNotReceive()
            .ListAsync(Arg.Any<LessonsCatalogSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStudentAcademicYearNotSet_ThrowsStudentAcademicYearNotSetException()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(academicYearId: null, teacher);

        _currentUser.UserId.Returns(StudentId);
        _studentRepository.GetByIdAsync(StudentId, Arg.Any<CancellationToken>()).Returns(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);

        await _lessonRepository
            .DidNotReceive()
            .ListAsync(Arg.Any<LessonsCatalogSpecification>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Status determination

    [Fact]
    public async Task Handle_WhenLessonHasNoEnrollment_ReturnsAvailableStatusWithFullPrice()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);
        var lesson = CreateLesson(id: 1, price: 250m);

        SetupStudentAndLessons(student, [lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!);
        Assert.Equal("avail", dto.Status);
        Assert.Equal(250m, dto.Price);
        Assert.Null(dto.PrerequisiteLabel);
        Assert.Null(dto.ExpiredDate);
    }

    [Fact]
    public async Task Handle_WhenEnrollmentExpired_ReturnsExpiredStatusWithZeroPriceAndArabicDateLabel()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);
        var lesson = CreateLesson(id: 1, price: 250m);
        lesson.Enrollments.Add(
            CreateEnrollment(
                StudentId,
                expiresAt: new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero)
            )
        );

        SetupStudentAndLessons(student, [lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!);
        Assert.Equal("expired", dto.Status);
        Assert.Equal(0m, dto.Price);
        Assert.Equal("انتهت صلاحيتك · انتهت في ١٥ يونيو", dto.ExpiredDate);
    }

    [Fact]
    public async Task Handle_WhenPrerequisiteNotCompleted_ReturnsLockedStatus()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);

        var prerequisiteLesson = CreateLesson(id: 1, price: 100m);
        var lesson = CreateLesson(id: 2, price: 300m, prerequisiteId: prerequisiteLesson.Id);

        lesson.Enrollments.Add(CreateEnrollment(StudentId));
        prerequisiteLesson.Enrollments.Add(CreateEnrollment(StudentId, isCompleted: false));

        SetupStudentAndLessons(student, [prerequisiteLesson, lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = result.Value!.Single(d => d.Id == lesson.Id);
        Assert.Equal("locked", dto.Status);
        Assert.Equal("تحتاج لإكمال الدرس السابق", dto.PrerequisiteLabel);
        Assert.Equal(0m, dto.Price);
    }

    [Fact]
    public async Task Handle_WhenPrerequisiteCompleted_ReturnsPurchasedStatus()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);

        var prerequisiteLesson = CreateLesson(id: 1, price: 100m);
        var lesson = CreateLesson(id: 2, price: 300m, prerequisiteId: prerequisiteLesson.Id);

        lesson.Enrollments.Add(CreateEnrollment(StudentId));
        prerequisiteLesson.Enrollments.Add(CreateEnrollment(StudentId, isCompleted: true));

        SetupStudentAndLessons(student, [prerequisiteLesson, lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = result.Value!.Single(d => d.Id == lesson.Id);
        Assert.Equal("purchased", dto.Status);
        Assert.Null(dto.PrerequisiteLabel);
    }

    [Fact]
    public async Task Handle_WhenActiveEnrollmentHasNoPrerequisite_ReturnsPurchasedStatus()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);
        var lesson = CreateLesson(id: 1, price: 300m);
        lesson.Enrollments.Add(CreateEnrollment(StudentId));

        SetupStudentAndLessons(student, [lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!);
        Assert.Equal("purchased", dto.Status);
    }

    #endregion

    #region Mapping details

    [Fact]
    public async Task Handle_MapsTeacherNameAndSubjectCorrectly()
    {
        // Arrange
        var teacher = CreateTeacher(firstName: "Mona", lastName: "Kamal", subject: "Physics");
        var student = CreateStudent(AcademicYearId, teacher);
        var lesson = CreateLesson(id: 1);

        SetupStudentAndLessons(student, [lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!);
        Assert.Equal("Mona Kamal", dto.TeacherName);
        Assert.Equal("Physics", dto.Subject);
    }

    [Fact]
    public async Task Handle_WhenThumbnailUrlIsNull_ReturnsEmptyImageUrlWithoutCallingStorageService()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);
        var lesson = CreateLesson(id: 1, thumbnailUrl: null);

        SetupStudentAndLessons(student, [lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!);
        Assert.Equal(string.Empty, dto.ImageThumbnailUrl);

        _storageService.DidNotReceive().GetDownloadUrlAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenThumbnailUrlIsSet_ReturnsPublicUrlFromStorageService()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);
        var lesson = CreateLesson(id: 1, thumbnailUrl: "lessons/1/thumb.png");

        _storageService
            .GetDownloadUrlAsync("prisma", "lessons/1/thumb.png")
            .Returns("https://cdn.example.com/prisma/lessons/1/thumb.png");

        SetupStudentAndLessons(student, [lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!);
        Assert.Equal("https://cdn.example.com/prisma/lessons/1/thumb.png", dto.ImageThumbnailUrl);
    }

    [Fact]
    public async Task Handle_MapsDurationHoursWithRounding()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);
        var lesson = CreateLesson(id: 1, duration: TimeSpan.FromMinutes(170)); // 2h 50m

        SetupStudentAndLessons(student, [lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!);
        Assert.Equal(3, dto.DurationHours);
    }

    [Fact]
    public async Task Handle_MapsDurationHoursWithRoundingDown()
    {
        // Arrange
        var teacher = CreateTeacher();
        var student = CreateStudent(AcademicYearId, teacher);
        var lesson = CreateLesson(id: 1, duration: TimeSpan.FromMinutes(140)); // 2h 20m

        SetupStudentAndLessons(student, [lesson]);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!);
        Assert.Equal(2, dto.DurationHours);
    }

    #endregion
}
