using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Assignments.Commands.GradeAssignmentSubmission;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assignments;

namespace Prisma.Application.Tests.Features.Assignments.Commands.GradeAssignmentSubmission;


public class GradeAssignmentSubmissionCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IRepository<AssignmentSubmission, int> _submissionRepository =
        Substitute.For<IRepository<AssignmentSubmission, int>>();

    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly GradeAssignmentSubmissionCommandHandler _handler;

    public GradeAssignmentSubmissionCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>().Returns(_submissionRepository);
        _currentUserService.UserId.Returns(_currentUserId);

        _handler = new GradeAssignmentSubmissionCommandHandler(_unitOfWork, _currentUserService);
    }

    // ---------- Helpers ----------

    private static AssignmentSubmission CreateSubmission(
        int id = 1,
        int maxScore = 100,
        int? score = null,
        string? notes = null,
        bool isBeingGraded = false,
        DateTimeOffset? gradingStartedAt = null,
        Guid? gradingByUserId = null)
    {
        var assignment = new Assignment { Id = 10, Grade = maxScore };

        return new AssignmentSubmission
        {
            Id = id,
            AssignmentId = assignment.Id,
            Assignment = assignment,
            Score = score,
            Notes = notes,
            IsBeingGraded = isBeingGraded,
            GradingStartedAt = gradingStartedAt,
            GradingByUserId = gradingByUserId,
            SubmittedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
    }

    private void SetupSubmission(AssignmentSubmission? submission) =>
        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionWithAssignmentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(submission);

    // ---------- Tests ----------

    [Fact]
    public async Task Handle_WhenSubmissionNotFound_ReturnsFailureWithoutSaving()
    {
        // Arrange
        SetupSubmission(null);

        // Act
        var result = await _handler.Handle(
            new GradeAssignmentSubmissionCommand(999, 50, "Good"), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("التسليم غير موجود");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenScoreExceedsMaxGrade_ReturnsFailureWithoutSaving()
    {
        // Arrange
        var submission = CreateSubmission(maxScore: 50);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new GradeAssignmentSubmissionCommand(submission.Id, 75, "Too high"), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("الدرجة (75) أكبر من الدرجة الكاملة (50)");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLockedByAnotherUserWithinWindow_ReturnsFailureWithoutSaving()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var submission = CreateSubmission(
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            gradingByUserId: otherUserId);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new GradeAssignmentSubmissionCommand(submission.Id, 50, "Good"), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("التسليم ده بيتصحح دلوقتي من شخص تاني");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        submission.Score.Should().BeNull(); // grading not applied
    }

    [Fact]
    public async Task Handle_WhenLockedByCurrentUser_SucceedsAndReleasesLock()
    {
        // Arrange
        var submission = CreateSubmission(
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            gradingByUserId: _currentUserId);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new GradeAssignmentSubmissionCommand(submission.Id, 90, "Great job"), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be("تم حفظ التصحيح بنجاح");

        submission.Score.Should().Be(90);
        submission.Notes.Should().Be("Great job");
        submission.IsBeingGraded.Should().BeFalse();
        submission.GradingStartedAt.Should().BeNull();
        submission.GradingByUserId.Should().BeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLockExpired_BypassesOtherUsersLockAndSucceeds()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var submission = CreateSubmission(
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-31),
            gradingByUserId: otherUserId);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new GradeAssignmentSubmissionCommand(submission.Id, 60, null), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        submission.Score.Should().Be(60);
        submission.IsBeingGraded.Should().BeFalse();
        submission.GradingByUserId.Should().BeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSubmissionNotCurrentlyBeingGraded_SucceedsDirectly()
    {
        // Arrange
        var submission = CreateSubmission(isBeingGraded: false);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new GradeAssignmentSubmissionCommand(submission.Id, 40, "Note"), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        submission.Score.Should().Be(40);
        submission.Notes.Should().Be("Note");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenScoreEqualsMaxGrade_Succeeds()
    {
        // Arrange
        var submission = CreateSubmission(maxScore: 100);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new GradeAssignmentSubmissionCommand(submission.Id, 100, "Perfect"), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        submission.Score.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WhenNoteIsNull_OverwritesExistingNoteWithNull()
    {
        // Arrange
        var submission = CreateSubmission(notes: "old note");
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new GradeAssignmentSubmissionCommand(submission.Id, 50, null), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        submission.Notes.Should().BeNull();
    }
}