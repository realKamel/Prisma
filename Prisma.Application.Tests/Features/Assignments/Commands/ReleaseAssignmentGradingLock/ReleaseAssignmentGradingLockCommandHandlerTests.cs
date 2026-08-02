

using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Assignments.Commands.ReleaseAssignmentGradingLock;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assignments;

namespace Prisma.Application.Tests.Features.Assignments.Commands.ReleaseAssignmentGradingLock;


public class ReleaseAssignmentGradingLockCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IRepository<AssignmentSubmission, int> _submissionRepository =
        Substitute.For<IRepository<AssignmentSubmission, int>>();

    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly ReleaseAssignmentGradingLockCommandHandler _handler;

    public ReleaseAssignmentGradingLockCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>().Returns(_submissionRepository);
        _currentUserService.UserId.Returns(_currentUserId);

        _handler = new ReleaseAssignmentGradingLockCommandHandler(_unitOfWork, _currentUserService);
    }

    // ---------- Helpers ----------

    private static AssignmentSubmission CreateSubmission(
        int id = 1,
        bool isBeingGraded = false,
        DateTimeOffset? gradingStartedAt = null,
        Guid? gradingByUserId = null) =>
        new()
        {
            Id = id,
            AssignmentId = 10,
            IsBeingGraded = isBeingGraded,
            GradingStartedAt = gradingStartedAt,
            GradingByUserId = gradingByUserId,
            SubmittedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

    private void SetupSubmission(AssignmentSubmission? submission) =>
        _submissionRepository
            .FirstOrDefaultAsync(Arg.Any<SubmissionByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(submission);

    // ---------- Tests ----------

    [Fact]
    public async Task Handle_WhenSubmissionNotFound_ReturnsFailureWithoutSaving()
    {
        // Arrange
        SetupSubmission(null);

        // Act
        var result = await _handler.Handle(
            new ReleaseAssignmentGradingLockCommand(999), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.GetResultMessage().Should().Be("التسليم غير موجود");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSubmissionIsNotLocked_ReturnsSuccessIdempotentlyWithoutSaving()
    {
        // Arrange
        var submission = CreateSubmission(isBeingGraded: false);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new ReleaseAssignmentGradingLockCommand(submission.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.GetResultMessage().Should().Be("القفل غير موجود بالفعل");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLockedByAnotherUser_ReturnsFailureWithoutReleasingLock()
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
            new ReleaseAssignmentGradingLockCommand(submission.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.GetResultMessage().Should().Be("مينفعش تفكي قفل تصحيح شخص تاني");

        submission.IsBeingGraded.Should().BeTrue();
        submission.GradingByUserId.Should().Be(otherUserId);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLockedByCurrentUser_ReleasesLockAndSaves()
    {
        // Arrange
        var submission = CreateSubmission(
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            gradingByUserId: _currentUserId);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new ReleaseAssignmentGradingLockCommand(submission.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.GetResultMessage().Should().Be("تم إلغاء قفل التصحيح");

        submission.IsBeingGraded.Should().BeFalse();
        submission.GradingStartedAt.Should().BeNull();
        submission.GradingByUserId.Should().BeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotConsiderLockExpiry_StillRequiresOwnershipEvenAfterThirtyMinutes()
    {
        // Arrange: lock started 40 minutes ago by another user (would be "expired" under the
        // grading-lock convention used elsewhere), but this handler has no such check —
        // ownership is still required to release it.
        var otherUserId = Guid.NewGuid();
        var submission = CreateSubmission(
            isBeingGraded: true,
            gradingStartedAt: DateTimeOffset.UtcNow.AddMinutes(-40),
            gradingByUserId: otherUserId);
        SetupSubmission(submission);

        // Act
        var result = await _handler.Handle(
            new ReleaseAssignmentGradingLockCommand(submission.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.GetResultMessage().Should().Be("مينفعش تفكي قفل تصحيح شخص تاني");

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}