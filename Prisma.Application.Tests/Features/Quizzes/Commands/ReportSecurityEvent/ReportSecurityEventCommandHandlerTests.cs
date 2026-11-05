using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Commands.ReportSecurityEvent;
using Prisma.Domain.Enums;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.ReportSecurityEvent;

public class ReportSecurityEventCommandHandlerTests
{
    private readonly IAttemptSecurityCounter _counter;
    private readonly ICurrentUserService _currentUser;
    private readonly ReportSecurityEventCommandHandler _handler;

    private static readonly Guid StudentId = Guid.NewGuid();

    public ReportSecurityEventCommandHandlerTests()
    {
        _counter = Substitute.For<IAttemptSecurityCounter>();
        _currentUser = Substitute.For<ICurrentUserService>();

        _currentUser.UserId.Returns(StudentId);

        _handler = new ReportSecurityEventCommandHandler(_counter, _currentUser);
    }

    #region Success

    [Fact]
    public async Task Handle_WhenCalled_ReturnsSuccess()
    {
        // Arrange
        var command = new ReportSecurityEventCommand(
            1,
            SecurityEventType.TabSwitch
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Counter Delegation

    [Theory]
    [InlineData(SecurityEventType.TabSwitch)]
    [InlineData(SecurityEventType.CopyPasteAttempt)]
    public async Task Handle_WhenSecurityEventReported_CallsCounterWithCorrectData(
        SecurityEventType eventType)
    {
        // Arrange
        var attemptId = 1;
        var ct = CancellationToken.None;

        var command = new ReportSecurityEventCommand(attemptId, eventType);

        // Act
        await _handler.Handle(command, ct);

        // Assert
        await _counter.Received(1).IncrementAsync(
            attemptId,
            StudentId,
            eventType,
            ct
        );
    }

    [Fact]
    public async Task Handle_WhenCounterReturnsFalse_ReturnsSuccess()
    {
        // Arrange
        _counter.IncrementAsync(
            Arg.Any<int>(),
            Arg.Any<Guid>(),
            Arg.Any<SecurityEventType>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(false));

        var command = new ReportSecurityEventCommand(
            1,
            SecurityEventType.TabSwitch
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion
}
