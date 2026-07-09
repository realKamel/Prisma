using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Commands.ReportSecurityEvent;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.ReportSecurityEvent;


public class ReportSecurityEventCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<QuizAttempt, int> _attemptRepository = Substitute.For<IRepository<QuizAttempt, int>>();
    private readonly ReportSecurityEventCommandHandler _handler;

    private static readonly Guid StudentId = Guid.NewGuid();

    public ReportSecurityEventCommandHandlerTests()
    {
        _currentUser.UserId.Returns(StudentId);
        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_attemptRepository);

        _handler = new ReportSecurityEventCommandHandler(_unitOfWork, _currentUser);
    }

    #region Helpers

    private static QuizAttempt CreateAttempt(
        QuizAttemptStatus status = QuizAttemptStatus.InProgress,
        int tabSwitchCount = 0,
        int copyPasteCount = 0) =>
        new()
        {
            Id = 1,
            StudentId = StudentId,
            Status = status,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            TabSwitchCount = tabSwitchCount,
            CopyPasteAttemptCount = copyPasteCount
        };

    private void SetupAttempt(QuizAttempt? attempt) =>
        _attemptRepository
            .FirstOrDefaultAsync(Arg.Any<AttemptByIdAndStudentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(attempt);

    #endregion

    #region Silent no-op cases

    [Fact]
    public async Task Handle_WhenAttemptNotFound_ReturnsSuccessWithoutError()
    {
        // Arrange
        SetupAttempt(null);

        // Act
        var result = await _handler.Handle(
            new ReportSecurityEventCommand(1, SecurityEventType.TabSwitch), CancellationToken.None);

        // Assert - silent success by design; the frontend fires these fire-and-forget
        Assert.True(result.Succeeded);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(QuizAttemptStatus.Submitted)]
    [InlineData(QuizAttemptStatus.Graded)]
    public async Task Handle_WhenAttemptNotInProgress_ReturnsSuccessWithoutIncrementingCounts(QuizAttemptStatus status)
    {
        // Arrange
        var attempt = CreateAttempt(status: status, tabSwitchCount: 2);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(
            new ReportSecurityEventCommand(1, SecurityEventType.TabSwitch), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, attempt.TabSwitchCount); // untouched

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Event counting

    [Fact]
    public async Task Handle_WhenTabSwitchEvent_IncrementsTabSwitchCountOnly()
    {
        // Arrange
        var attempt = CreateAttempt();
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(
            new ReportSecurityEventCommand(1, SecurityEventType.TabSwitch), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, attempt.TabSwitchCount);
        Assert.Equal(0, attempt.CopyPasteAttemptCount);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCopyPasteEvent_IncrementsCopyPasteCountOnly()
    {
        // Arrange
        var attempt = CreateAttempt();
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(
            new ReportSecurityEventCommand(1, SecurityEventType.CopyPasteAttempt), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, attempt.CopyPasteAttemptCount);
        Assert.Equal(0, attempt.TabSwitchCount);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMultipleEventsReportedSequentially_AccumulatesCount()
    {
        // Arrange
        var attempt = CreateAttempt(tabSwitchCount: 3);
        SetupAttempt(attempt);

        // Act
        await _handler.Handle(new ReportSecurityEventCommand(1, SecurityEventType.TabSwitch), CancellationToken.None);

        // Assert
        Assert.Equal(4, attempt.TabSwitchCount);
    }

    #endregion
}
