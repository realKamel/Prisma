using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Assignments.Queries.GetAssignmentSubmissionsList;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.Assignments.Queries.GetAssignmentSubmissionsList;


public class GetAssignmentSubmissionsListQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly IRepository<Assignment, int> _assignmentRepository = Substitute.For<IRepository<Assignment, int>>();
    private readonly IRepository<Enrollment, int> _enrollmentRepository = Substitute.For<IRepository<Enrollment, int>>();
    private readonly IRepository<AssignmentSubmission, int> _submissionRepository = Substitute.For<IRepository<AssignmentSubmission, int>>();
    private readonly IRepository<User, Guid> _userRepository = Substitute.For<IRepository<User, Guid>>();

    private readonly GetAssignmentSubmissionsListQueryHandler _handler;

    public GetAssignmentSubmissionsListQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Assignment, int>().Returns(_assignmentRepository);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepository);
        _unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>().Returns(_submissionRepository);
        _unitOfWork.GetOrCreateRepository<User, Guid>().Returns(_userRepository);

        _currentUserService.UserId.Returns(Guid.NewGuid());

        _handler = new GetAssignmentSubmissionsListQueryHandler(_unitOfWork, _currentUserService);
    }

    // ---------- Helpers ----------

    private static GetAssignmentSubmissionsListQuery CreateQuery(
        string? search = null, int? lessonId = null, string? status = null, int page = 1, int pageSize = 20) =>
        new(search, lessonId, status, page, pageSize);

    private static Lesson CreateLesson(int id, string title = "Lesson") => new() { Id = id, Title = title };

    private static Assignment CreateAssignment(int id, int lessonId, Lesson lesson, int grade = 100) =>
        new() { Id = id, LessonId = lessonId, Lesson = lesson, Grade = grade };

    private static Student CreateStudent(string firstName = "John", string lastName = "Doe") =>
        new() { Id = Guid.NewGuid(), FirstName = firstName, LastName = lastName };

    private static Enrollment CreateEnrollment(int lessonId, Student student) =>
        new() { LessonId = lessonId, Student = student };

    private static AssignmentSubmission CreateSubmission(
        int id,
        int assignmentId,
        Guid studentId,
        int? score = null,
        bool isBeingGraded = false,
        DateTimeOffset? gradingStartedAt = null,
        Guid? gradingByUserId = null,
        DateTimeOffset? submittedAt = null,
        string? fileUrl = "file.pdf") =>
        new()
        {
            Id = id,
            AssignmentId = assignmentId,
            StudentId = studentId,
            Score = score,
            IsBeingGraded = isBeingGraded,
            GradingStartedAt = gradingStartedAt,
            GradingByUserId = gradingByUserId,
            SubmittedAt = submittedAt ?? DateTimeOffset.UtcNow.AddDays(-1),
            FileUrl = fileUrl
        };

    private void SetupRepositories(
        List<Assignment> assignments,
        List<Enrollment>? enrollments = null,
        List<AssignmentSubmission>? submissions = null,
        List<User>? gradingUsers = null)
    {
        _assignmentRepository
            .ListAsync(Arg.Any<ISpecification<Assignment>>(), Arg.Any<CancellationToken>())
            .Returns(assignments);

        _enrollmentRepository
            .ListAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(enrollments ?? new List<Enrollment>());

        _submissionRepository
            .ListAsync(Arg.Any<ISpecification<AssignmentSubmission>>(), Arg.Any<CancellationToken>())
            .Returns(submissions ?? new List<AssignmentSubmission>());

        _userRepository
            .ListAsync(Arg.Any<ISpecification<User>>(), Arg.Any<CancellationToken>())
            .Returns(gradingUsers ?? new List<User>());
    }

    // ---------- Tests ----------

    [Fact]
    public async Task Handle_WhenNoAssignmentsExist_ReturnsEmptyResponseWithoutFurtherQueries()
    {
        // Arrange
        SetupRepositories(new List<Assignment>());

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);

        await _enrollmentRepository.DidNotReceive()
            .ListAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>());
        await _submissionRepository.DidNotReceive()
            .ListAsync(Arg.Any<ISpecification<AssignmentSubmission>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonIdProvided_FiltersOutAssignmentsFromOtherLessons()
    {
        // Arrange
        var lessonA = CreateLesson(1, "Lesson A");
        var lessonB = CreateLesson(2, "Lesson B");
        var assignmentA = CreateAssignment(10, lessonA.Id, lessonA);
        var assignmentB = CreateAssignment(20, lessonB.Id, lessonB);

        var studentA = CreateStudent("Alice", "A");
        var enrollmentA = CreateEnrollment(lessonA.Id, studentA);

        SetupRepositories(
            new List<Assignment> { assignmentA, assignmentB },
            new List<Enrollment> { enrollmentA });

        // Act
        var result = await _handler.Handle(CreateQuery(lessonId: lessonA.Id), CancellationToken.None);

        // Assert
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].AssignmentId.Should().Be(assignmentA.Id);
        result.Value.Items[0].LessonTitle.Should().Be("Lesson A");
    }

    [Fact]
    public async Task Handle_WhenAssignmentLessonHasNoEnrollments_SkipsAssignmentEntirely()
    {
        // Arrange
        var lesson = CreateLesson(1, "Lonely Lesson");
        var assignment = CreateAssignment(10, lesson.Id, lesson);

        // No enrollments for lesson.Id at all
        SetupRepositories(new List<Assignment> { assignment });

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenStudentHasNoSubmission_StatusIsNotSubmitted()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson, grade: 50);
        var student = CreateStudent("John", "Doe");
        var enrollment = CreateEnrollment(lesson.Id, student);

        SetupRepositories(
            new List<Assignment> { assignment },
            new List<Enrollment> { enrollment });

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.Status.Should().Be("not_submitted");
        item.SubmissionId.Should().Be(0);
        item.Score.Should().BeNull();
        item.SubmittedAt.Should().BeNull();
        item.FileUrl.Should().BeNull();
        item.MaxScore.Should().Be(50);
        item.IsBeingGraded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenSubmissionExistsWithoutScoreAndNotBeingGraded_StatusIsPending()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);
        var student = CreateStudent();
        var enrollment = CreateEnrollment(lesson.Id, student);
        var submission = CreateSubmission(100, assignment.Id, student.Id);

        SetupRepositories(
            new List<Assignment> { assignment },
            new List<Enrollment> { enrollment },
            new List<AssignmentSubmission> { submission });

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.Status.Should().Be("pending");
        item.SubmissionId.Should().Be(submission.Id);
        item.IsBeingGraded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenSubmissionHasScore_StatusIsGraded()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);
        var student = CreateStudent();
        var enrollment = CreateEnrollment(lesson.Id, student);
        var submission = CreateSubmission(100, assignment.Id, student.Id, score: 85);

        SetupRepositories(
            new List<Assignment> { assignment },
            new List<Enrollment> { enrollment },
            new List<AssignmentSubmission> { submission });

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.Status.Should().Be("graded");
        item.Score.Should().Be(85);
    }

    [Fact]
    public async Task Handle_WhenSubmissionIsBeingGradedWithinLockWindow_StatusIsGradingAndIncludesGraderName()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);
        var student = CreateStudent();
        var enrollment = CreateEnrollment(lesson.Id, student);

        var graderId = Guid.NewGuid();
        var grader = new User { Id = graderId, FirstName = "Jane", LastName = "Grader" };

        var submission = CreateSubmission(
            100, assignment.Id, student.Id,
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            gradingByUserId: graderId);

        SetupRepositories(
            new List<Assignment> { assignment },
            new List<Enrollment> { enrollment },
            new List<AssignmentSubmission> { submission },
            new List<User> { grader });

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.Status.Should().Be("grading");
        item.IsBeingGraded.Should().BeTrue();
        item.GradingByUserName.Should().Be("Jane Grader");
    }

    [Fact]
    public async Task Handle_WhenGradingLockHasExpired_FallsBackToScoreBasedStatus()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);
        var student = CreateStudent();
        var enrollment = CreateEnrollment(lesson.Id, student);

        var graderId = Guid.NewGuid();
        var grader = new User { Id = graderId, FirstName = "Jane", LastName = "Grader" };

        // Grading started 31 minutes ago -> lock expired (GradingLockExpiry is 30 min)
        var submission = CreateSubmission(
            100, assignment.Id, student.Id,
            score: null,
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-31),
            gradingByUserId: graderId);

        SetupRepositories(
            new List<Assignment> { assignment },
            new List<Enrollment> { enrollment },
            new List<AssignmentSubmission> { submission },
            new List<User> { grader });

        // Act
        var result = await _handler.Handle(CreateQuery(), CancellationToken.None);

        // Assert
        var item = result.Value.Items.Should().ContainSingle().Subject;
        // Lock expired and no score recorded yet -> falls back to "pending", not "grading"
        item.Status.Should().Be("pending");
        item.IsBeingGraded.Should().BeFalse();
        item.GradingByUserName.Should().BeNull();
    }
    [Fact]
    public async Task Handle_WhenSearchProvided_FiltersItemsByStudentNameCaseInsensitively()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);

        var alice = CreateStudent("Alice", "Anderson");
        var bob = CreateStudent("Bob", "Brown");

        var enrollments = new List<Enrollment>
        {
            CreateEnrollment(lesson.Id, alice),
            CreateEnrollment(lesson.Id, bob)
        };

        SetupRepositories(new List<Assignment> { assignment }, enrollments);

        // Act
        var result = await _handler.Handle(CreateQuery(search: "alice"), CancellationToken.None);

        // Assert
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.StudentName.Should().Be("Alice Anderson");
    }

    [Fact]
    public async Task Handle_WhenStatusFilterProvided_ReturnsOnlyMatchingItems()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);

        var gradedStudent = CreateStudent("Graded", "Student");
        var notSubmittedStudent = CreateStudent("NotSubmitted", "Student");

        var enrollments = new List<Enrollment>
        {
            CreateEnrollment(lesson.Id, gradedStudent),
            CreateEnrollment(lesson.Id, notSubmittedStudent)
        };

        var submissions = new List<AssignmentSubmission>
        {
            CreateSubmission(100, assignment.Id, gradedStudent.Id, score: 90)
        };

        SetupRepositories(new List<Assignment> { assignment }, enrollments, submissions);

        // Act
        var result = await _handler.Handle(CreateQuery(status: "graded"), CancellationToken.None);

        // Assert
        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.StudentName.Should().Be("Graded Student");
        item.Status.Should().Be("graded");
    }

    [Fact]
    public async Task Handle_WhenStatusFilterIsAll_ReturnsItemsOfEveryStatus()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);

        var gradedStudent = CreateStudent("Graded", "Student");
        var notSubmittedStudent = CreateStudent("NotSubmitted", "Student");

        var enrollments = new List<Enrollment>
        {
            CreateEnrollment(lesson.Id, gradedStudent),
            CreateEnrollment(lesson.Id, notSubmittedStudent)
        };

        var submissions = new List<AssignmentSubmission>
        {
            CreateSubmission(100, assignment.Id, gradedStudent.Id, score: 90)
        };

        SetupRepositories(new List<Assignment> { assignment }, enrollments, submissions);

        // Act
        var result = await _handler.Handle(CreateQuery(status: "all"), CancellationToken.None);

        // Assert
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_AppliesPaginationCorrectly()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);

        var students = Enumerable.Range(1, 5)
            .Select(i => CreateStudent($"Student{i}", "Test"))
            .ToList();

        var enrollments = students
            .Select(s => CreateEnrollment(lesson.Id, s))
            .ToList();

        SetupRepositories(new List<Assignment> { assignment }, enrollments);

        // Act
        var result = await _handler.Handle(CreateQuery(page: 2, pageSize: 2), CancellationToken.None);

        // Assert
        result.Value.TotalCount.Should().Be(5);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenPageSizeExceedsMax_ClampsToOneHundred()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);
        var student = CreateStudent();
        var enrollment = CreateEnrollment(lesson.Id, student);

        SetupRepositories(new List<Assignment> { assignment }, new List<Enrollment> { enrollment });

        // Act
        var result = await _handler.Handle(CreateQuery(pageSize: 500), CancellationToken.None);

        // Assert
        result.Value.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WhenPageIsLessThanOne_ClampsToFirstPage()
    {
        // Arrange
        var lesson = CreateLesson(1);
        var assignment = CreateAssignment(10, lesson.Id, lesson);
        var student = CreateStudent();
        var enrollment = CreateEnrollment(lesson.Id, student);

        SetupRepositories(new List<Assignment> { assignment }, new List<Enrollment> { enrollment });

        // Act
        var result = await _handler.Handle(CreateQuery(page: 0), CancellationToken.None);

        // Assert
        result.Value.Page.Should().Be(1);
    }
}
