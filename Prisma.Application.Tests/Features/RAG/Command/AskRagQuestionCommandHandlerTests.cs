using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RAG.Commands.AskRagQuestion;
using Prisma.Application.Features.RAG.Dto;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.RAG.Command;

public class AskRagQuestionCommandHandlerTests
{
    private readonly IRagQuestionAnswering _ragService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _uow;
    private readonly IRepository<ChatSession, Guid> _sessionRepo;
    private readonly AskRagQuestionCommandHandler _handler;

    public AskRagQuestionCommandHandlerTests()
    {
        // 1. Setup Mock Dependencies
        _ragService = Substitute.For<IRagQuestionAnswering>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _uow = Substitute.For<IUnitOfWork>();
        _sessionRepo = Substitute.For<IRepository<ChatSession, Guid>>();

        // Configure Unit of Work to return our mocked repository
        _uow.GetOrCreateRepository<ChatSession, Guid>().Returns(_sessionRepo);

        // Mock a default User ID
        _currentUserService.UserId.Returns(Guid.NewGuid());

        // 2. Instantiate the Handler
        _handler = new AskRagQuestionCommandHandler(_ragService, _currentUserService, _uow);
    }

    [Fact]
    public async Task Handle_ShouldStreamChunksAndSaveSession_WhenSessionIdIsNull()
    {
        // Arrange
        var command = new AskRagQuestionCommand
        (
            SessionId: null,
            Question: "What is Angular 20?"
        );

        // Create mock streaming chunks that the RAG service will emit
        var mockChunks = new List<RagAnswer>
        {
            new(Text: "Angular ", ThreadState: "state-1", null),
            new(Text: "20 ", ThreadState: "state-2", null),
            new(Text: "is zoneless.", ThreadState: "state-final", null)
        };

        _ragService.AskAsync(command.Question, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => mockChunks.ToAsyncEnumerable());

        // Act
        // Call the handler to retrieve the IAsyncEnumerable stream
        var streamResult = _handler.Handle(command, CancellationToken.None);

        var receivedChunks = new List<Result<AskRagQuestionCommandResponse>>();

        // Consume the asynchronous stream exactly like your API Controller does
        await foreach (var item in streamResult)
        {
            receivedChunks.Add(item);
        }

        // Assert
        // 1. Verify stream contents and structure
        receivedChunks.Should().HaveCount(3);
        receivedChunks[0].Data.Answer.Should().Be("Angular ");
        receivedChunks[1].Data.Answer.Should().Be("20 ");
        receivedChunks[2].Data.Answer.Should().Be("is zoneless.");

        // 2. Verify Database Interactions
        // Check that a brand-new session was added since command.SessionId was null
        _sessionRepo.Received(1).Add(Arg.Any<ChatSession>());

        // Check that the session was updated with the final dynamic thread state
        _sessionRepo.Received(1)
            .Update(Arg.Is<ChatSession>(s => s.SerializedSessionJson == "state-final"));

        // Ensure changes were saved to the database twice (once at the start, once at the end)
        await _uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUseExistingSession_WhenSessionIdIsProvided()
    {
        // Arrange
        var existingSessionId = Guid.CreateVersion7();
        var command = new AskRagQuestionCommand(
            Question: "Continue chat",
            SessionId: existingSessionId
        );

        var existingSession = ChatSession
            .Create(_currentUserService.UserId, "Old Title", "initial-state");
        _sessionRepo
            .GetByIdAsync(existingSessionId, Arg.Any<CancellationToken>()).Returns(existingSession);

        var mockChunks = new List<RagAnswer>
        {
            new(Text: "Response token", ThreadState: "updated-state", null)
        };
        _ragService.AskAsync(command.Question, "initial-state",
                Arg.Any<CancellationToken>())
            .Returns(mockChunks.ToAsyncEnumerable());

        // Act
        var streamResult = _handler
            .Handle(command, CancellationToken.None);

        // Force evaluation of the stream to trigger execution code paths
        await foreach (var _ in streamResult)
        {
        }

        // Assert
        // Since the session already existed, it should NOT add a new one, only update metadata
        _sessionRepo.DidNotReceive()
            .Add(Arg.Any<ChatSession>());
        _sessionRepo.Received(1)
            .Update(Arg.Is<ChatSession>(s => s.SerializedSessionJson == "updated-state"));

        // SaveChangesAsync should only fire ONCE (at the very end of the stream)
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}