

using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Assignments.Queries.GetAssignmentSubmissionDetail;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assignments;

namespace Prisma.Application.Tests.Features.Assignments.Queries.GetAssignmentSubmissionDetail;


public class GetAssignmentSubmissionDetailQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IRepository<AssignmentSubmission, int> _submissionRepository =
        Substitute.For<IRepository<AssignmentSubmission, int>>();

    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly GetAssignmentSubmissionDetailQueryHandler _handler;

    public GetAssignmentSubmissionDetailQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>().Returns(_submissionRepository);
        _currentUserService.UserId.Returns(_currentUserId);

        _handler = new GetAssignmentSubmissionDetailQueryHandler(_unitOfWork, _currentUserService);
    }

    // ---------- Helpers ----------

    private static AssignmentSubmission CreateSubmission(
        int id = 1,
        int? score = null,
        string? notes = null,
        bool isBeingGraded = false,
        DateTimeOffset? gradingStartedAt = null,
        Guid? gradingByUserId = null,
        DateTimeOffset? submittedAt = null,
        DateTimeOffset? dueDate = null,
        int maxScore = 100,
        string studentFirstName = "John",
        string studentLastName = "Doe",
        string lessonTitle = "Lesson 1",
        string? fileUrl = "file.pdf")
    {
        var lesson = new Lesson { Id = 1, Title = lessonTitle };
        var assignment = new Assignment
        {
            Id = 10,
            LessonId = lesson.Id,
            Lesson = lesson,
            Grade = maxScore,
            DueDate = dueDate ?? DateTimeOffset.UtcNow.AddDays(3)
        };
        var student = new Student { Id = Guid.NewGuid(), FirstName = studentFirstName, LastName = studentLastName };

        return new AssignmentSubmission
        {
            Id = id,
            Student = student,
            StudentId = student.Id,
            Assignment = assignment,
            AssignmentId = assignment.Id,
            Score = score,
            Notes = notes,
            FileUrl = fileUrl,
            SubmittedAt = submittedAt ?? DateTimeOffset.UtcNow.AddDays(-1),
            IsBeingGraded = isBeingGraded,
            GradingStartedAt = gradingStartedAt,
            GradingByUserId = gradingByUserId
        };
    }

    // ---------- Tests ----------

    [Fact]
    public async Task Handle_WhenSubmissionNotFound_ReturnsFailureWithoutSaving()
    {
        // Arrange
        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionDetailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AssignmentSubmission?)null);

        // Act
        var result = await _handler.Handle(new GetAssignmentSubmissionDetailQuery(999), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("التسليم غير موجود");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSubmissionIsLockedByAnotherUserWithinWindow_ReturnsFailureWithoutSaving()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var submission = CreateSubmission(
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            gradingByUserId: otherUserId);

        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionDetailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(submission);

        // Act
        var result = await _handler.Handle(new GetAssignmentSubmissionDetailQuery(submission.Id), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("التسليم ده بيتصحح دلوقتي من شخص تاني");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        submission.GradingByUserId.Should().Be(otherUserId); // lock not stolen
    }

    [Fact]
    public async Task Handle_WhenGradingLockHasExpired_AcquiresLockAndSucceeds()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var submission = CreateSubmission(
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-31),
            gradingByUserId: otherUserId);

        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionDetailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(submission);

        // Act
        var result = await _handler.Handle(new GetAssignmentSubmissionDetailQuery(submission.Id), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();

        submission.IsBeingGraded.Should().BeTrue();
        submission.GradingByUserId.Should().Be(_currentUserId);
        submission.GradingStartedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSubmissionNotCurrentlyBeingGraded_AcquiresLockAndSucceeds()
    {
        // Arrange
        var submission = CreateSubmission(isBeingGraded: false);

        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionDetailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(submission);

        // Act
        var result = await _handler.Handle(new GetAssignmentSubmissionDetailQuery(submission.Id), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();

        submission.IsBeingGraded.Should().BeTrue();
        submission.GradingByUserId.Should().Be(_currentUserId);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsCorrectlyMappedDtoFields()
    {
        // Arrange
        var submittedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var dueDate = DateTimeOffset.UtcNow.AddDays(3);

        var submission = CreateSubmission(
            id: 42,
            score: 75,
            notes: "Good effort",
            submittedAt: submittedAt,
            dueDate: dueDate,
            maxScore: 100,
            studentFirstName: "Alice",
            studentLastName: "Anderson",
            lessonTitle: "Algebra Basics",
            fileUrl: "https://files.example.com/42.pdf");

        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionDetailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(submission);

        // Act
        var result = await _handler.Handle(new GetAssignmentSubmissionDetailQuery(submission.Id), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        var dto = result.Data;

        dto.SubmissionId.Should().Be(42);
        dto.StudentName.Should().Be("Alice Anderson");
        dto.LessonTitle.Should().Be("Algebra Basics");
        dto.SubmittedAt.Should().Be(submittedAt);
        dto.DueDate.Should().Be(dueDate);
        dto.IsLateSubmission.Should().BeFalse();
        dto.FileUrl.Should().Be("https://files.example.com/42.pdf");
        dto.MaxScore.Should().Be(100);
        dto.CurrentScore.Should().Be(75);
        dto.CurrentNote.Should().Be("Good effort");
    }

    [Fact]
    public async Task Handle_WhenSubmittedAfterDueDate_IsLateSubmissionIsTrue()
    {
        // Arrange
        var dueDate = DateTimeOffset.UtcNow.AddDays(-2);
        var submittedAt = DateTimeOffset.UtcNow.AddDays(-1); // after due date

        var submission = CreateSubmission(submittedAt: submittedAt, dueDate: dueDate);

        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionDetailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(submission);

        // Act
        var result = await _handler.Handle(new GetAssignmentSubmissionDetailQuery(submission.Id), CancellationToken.None);

        // Assert
        result.Data.IsLateSubmission.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSubmissionHasNoScoreYet_CurrentScoreIsNull()
    {
        // Arrange
        var submission = CreateSubmission(score: null, notes: null);

        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionDetailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(submission);

        // Act
        var result = await _handler.Handle(new GetAssignmentSubmissionDetailQuery(submission.Id), CancellationToken.None);

        // Assert
        result.Data.CurrentScore.Should().BeNull();
        result.Data.CurrentNote.Should().BeNull();
    }
}