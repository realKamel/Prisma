using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Queries.GetPaginatedLessonsCatalog;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.ValueObjects.ContentDomain;
using Prisma.Domain.ValueObjects.EnrollmentDomain;

namespace Prisma.Application.Tests.Features.LessonCatalog.Queries;

public class GetLessonsCatalogQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IRepository<Student, Guid> _studentRepository = Substitute.For<
        IRepository<Student, Guid>
    >();
    private readonly IRepository<Enrollment, int> _enrollmentRepository = Substitute.For<
        IRepository<Enrollment, int>
    >();
    private readonly GetPaginatedLessonsCatalogQueryHandler _handler;

    private static readonly Guid StudentId = Guid.NewGuid();
    private const int AcademicYearId = 1;

    public GetLessonsCatalogQueryHandlerTests()
    {
        _storageService.DefaultBucketName.Returns("prisma");
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepository);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepository);

        _handler = new GetPaginatedLessonsCatalogQueryHandler(
            _unitOfWork,
            _currentUser,
            _storageService
        );
    }

    #region Helpers

    private static GetPaginatedLessonsCatalogQuery CreateQuery(
        int pageNumber = 1,
        int pageSize = 10
    ) => new(new PaginationParams { PageNumber = pageNumber, PageSize = pageSize });

    private static Prisma.Domain.Entities.UserAggregate.Teacher CreateTeacher(
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

    private static Student CreateStudent(int? academicYearId) =>
        new()
        {
            Id = StudentId,
            FirstName = "Sara",
            SecondName = "Ali",
            LastName = "Hassan",
            AcademicYearId = academicYearId,
        };

    private static Lesson CreateLesson(
        int id,
        string title = "Lesson",
        long durationSeconds = 0,
        string? thumbnailUrl = null,
        int? prerequisiteId = null,
        Lesson? prerequisite = null
    ) =>
        new()
        {
            Id = id,
            Title = title,
            TimeDuration = TimeDuration.FromSeconds(durationSeconds).Value,
            ImageThumbnailUrl = thumbnailUrl,
            PrerequisiteId = prerequisiteId,
            Prerequisite = prerequisite,
        };

    private static Enrollment CreateEnrollment(
        Lesson lesson,
        EnrollmentStatus status = EnrollmentStatus.Active,
        bool isCompleted = false,
        DateTimeOffset? expiresAt = null
    )
    {
        var enrollment = new Enrollment
        {
            StudentId = StudentId,
            Lesson = lesson,
            Status = status,
            IsCompleted = isCompleted,
        };

        if (expiresAt is not null)
        {
            enrollment.ValidityDateRange = DateRange
                .Create(expiresAt.Value.AddDays(-30), expiresAt.Value)
                .Value;
        }

        return enrollment;
    }

    private void Setup(Student student, List<Enrollment> enrollments, int? totalCount = null)
    {
        _currentUser.UserId.Returns(StudentId);
        _studentRepository.GetByIdAsync(StudentId, Arg.Any<CancellationToken>()).Returns(student);
        _enrollmentRepository
            .ListAsync(Arg.Any<ActiveEnrollmentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(enrollments);
        _enrollmentRepository
            .CountAsync(Arg.Any<ActiveEnrollmentsSpec>(), Arg.Any<CancellationToken>())
            .Returns(totalCount ?? enrollments.Count);
    }

    #endregion

    #region Guards

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);

        await _studentRepository
            .DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _enrollmentRepository
            .DidNotReceive()
            .ListAsync(Arg.Any<ActiveEnrollmentsSpec>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStudentNotFound_ReturnsUnauthorized()
    {
        _currentUser.UserId.Returns(StudentId);
        _studentRepository
            .GetByIdAsync(StudentId, Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);

        await _enrollmentRepository
            .DidNotReceive()
            .ListAsync(Arg.Any<ActiveEnrollmentsSpec>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStudentHasNoAcademicYear_ReturnsNotFound()
    {
        var student = CreateStudent(academicYearId: null);
        Setup(student, [], totalCount: 0);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().Contain($"Student {StudentId} has no academic year assigned.");

        await _enrollmentRepository
            .DidNotReceive()
            .ListAsync(Arg.Any<ActiveEnrollmentsSpec>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Status determination

    [Fact]
    public async Task Handle_WhenEnrollmentExpired_ReturnsExpiredStatusAndExpiryDate()
    {
        var student = CreateStudent(AcademicYearId);
        var lesson = CreateLesson(id: 1, title: "Algebra");
        var expiresAt = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var enrollment = CreateEnrollment(
            lesson,
            status: EnrollmentStatus.Expired,
            expiresAt: expiresAt
        );
        Setup(student, [enrollment]);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        var dto = result.Value.Items.Single();
        dto.Status.Should().Be(EnrollmentStatus.Expired);
        dto.IsExpired.Should().BeTrue();
        dto.ExpiresAt.Should().Be(expiresAt);
        dto.PrerequisiteLabel.Should().BeNull();
        dto.Money.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPrerequisiteNotCompleted_ReturnsLockedStatusWithArabicLabel()
    {
        var student = CreateStudent(AcademicYearId);
        var prerequisite = CreateLesson(id: 1, title: "Basics");
        var lesson = CreateLesson(
            id: 2,
            title: "Advanced",
            prerequisiteId: 1,
            prerequisite: prerequisite
        );
        prerequisite.Enrollments.Add(CreateEnrollment(prerequisite, isCompleted: false));
        var enrollment = CreateEnrollment(lesson);
        Setup(student, [enrollment]);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        var dto = result.Value.Items.Single();
        dto.Status.Should().Be(EnrollmentStatus.Locked);
        dto.PrerequisiteLabel.Should().Be("تحتاج لإكمال الدرس السابق");
        dto.Money.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenPrerequisiteCompleted_ReturnsActiveStatusWithoutLabel()
    {
        var student = CreateStudent(AcademicYearId);
        var prerequisite = CreateLesson(id: 1, title: "Basics");
        var lesson = CreateLesson(
            id: 2,
            title: "Advanced",
            prerequisiteId: 1,
            prerequisite: prerequisite
        );
        prerequisite.Enrollments.Add(CreateEnrollment(prerequisite, isCompleted: true));
        var enrollment = CreateEnrollment(lesson);
        Setup(student, [enrollment]);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        var dto = result.Value.Items.Single();
        dto.Status.Should().Be(EnrollmentStatus.Active);
        dto.PrerequisiteLabel.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenActiveEnrollmentHasNoPrerequisite_ReturnsActiveStatus()
    {
        var student = CreateStudent(AcademicYearId);
        var lesson = CreateLesson(id: 1, title: "Algebra");
        var enrollment = CreateEnrollment(lesson);
        Setup(student, [enrollment]);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        var dto = result.Value.Items.Single();
        dto.Status.Should().Be(EnrollmentStatus.Active);
        dto.IsExpired.Should().BeFalse();
        dto.ExpiresAt.Should().BeNull();
    }

    #endregion

    #region Mapping details

    [Fact]
    public async Task Handle_MapsTeacherSubjectAndDuration()
    {
        var student = CreateStudent(AcademicYearId);
        var teacher = CreateTeacher(firstName: "Mona", lastName: "Kamal", subject: "Physics");
        var lesson = CreateLesson(
            id: 1,
            title: "Kinematics",
            durationSeconds: 7200,
            prerequisiteId: null
        );
        lesson.Teacher = teacher;
        var enrollment = CreateEnrollment(lesson);
        Setup(student, [enrollment]);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        var dto = result.Value.Items.Single();
        dto.PublicId.Should().Be(lesson.PublicId);
        dto.Title.Should().Be("Kinematics");
        dto.TeacherName.Should().Be("Mona Kamal");
        dto.Subject.Should().Be("Physics");
        dto.Duration.Seconds.Should().Be(7200);
    }

    [Fact]
    public async Task Handle_WhenThumbnailUrlIsNull_ReturnsEmptyImageUrlAndSkipsStorageCall()
    {
        var student = CreateStudent(AcademicYearId);
        var lesson = CreateLesson(id: 1, thumbnailUrl: null);
        var enrollment = CreateEnrollment(lesson);
        Setup(student, [enrollment]);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        var dto = result.Value.Items.Single();
        dto.ImageThumbnailUrl.Should().BeEmpty();

        await _storageService
            .DidNotReceive()
            .GetDownloadUrlAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Handle_WhenThumbnailUrlIsSet_ReturnsPublicUrlFromStorageService()
    {
        var student = CreateStudent(AcademicYearId);
        var lesson = CreateLesson(id: 1, thumbnailUrl: "lessons/1/thumb.png");
        var enrollment = CreateEnrollment(lesson);
        Setup(student, [enrollment]);

        _storageService
            .GetDownloadUrlAsync("prisma", "lessons/1/thumb.png")
            .Returns("https://cdn.example.com/prisma/lessons/1/thumb.png");

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        var dto = result.Value.Items.Single();
        dto.ImageThumbnailUrl.Should().Be("https://cdn.example.com/prisma/lessons/1/thumb.png");
    }

    [Fact]
    public async Task Handle_WhenEnrollmentHasNoLesson_SkipsIt()
    {
        var student = CreateStudent(AcademicYearId);
        var lesson = CreateLesson(id: 1, title: "Algebra");
        var mapped = CreateEnrollment(lesson);
        var orphan = new Enrollment { StudentId = StudentId, Lesson = null };
        Setup(student, [mapped, orphan]);

        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.Single().Title.Should().Be("Algebra");
    }

    #endregion

    #region Pagination

    [Fact]
    public async Task Handle_ReturnsPaginatedMetadataForRequestedPage()
    {
        var student = CreateStudent(AcademicYearId);
        var lessons = new List<Enrollment>
        {
            CreateEnrollment(CreateLesson(id: 1, title: "Lesson 1")),
            CreateEnrollment(CreateLesson(id: 2, title: "Lesson 2")),
        };
        Setup(student, lessons, totalCount: 7);

        var result = await _handler.Handle(
            CreateQuery(pageNumber: 2, pageSize: 5),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        var page = result.Value;
        page.Items.Select(dto => dto.Title).Should().Equal("Lesson 1", "Lesson 2");
        page.TotalCount.Should().Be(7);
        page.PageNumber.Should().Be(2);
        page.PageSize.Should().Be(5);
        page.TotalPages.Should().Be(2);
        page.HasPreviousPage.Should().BeTrue();
        page.HasNextPage.Should().BeFalse();

        await _enrollmentRepository
            .Received(1)
            .CountAsync(Arg.Any<ActiveEnrollmentsSpec>(), Arg.Any<CancellationToken>());
    }

    #endregion
}
