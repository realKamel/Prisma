using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Chats.Commands.SendMessage;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Tests.Features.Chats;

public class SendMessageCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();

    private readonly IConversationRepository _conversationRepository =
        Substitute.For<IConversationRepository>();

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SendMessageCommandHandler _sut;

    public SendMessageCommandHandlerTests()
    {
        _sut = new SendMessageCommandHandler(
            _currentUserService,
            _conversationRepository,
            _unitOfWork
        );
    }

    [Fact]
    public async Task Handle_WhenUserIsUnauthenticated_ReturnsUnauthorized()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        var result = await _sut.Handle(
            new SendMessageCommand(Guid.NewGuid(), "hello"),
            CancellationToken.None
        );

        result.Status.Should().Be(ResultStatus.Unauthorized);
        _conversationRepository.DidNotReceive().AddMessage(Arg.Any<Message>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConversationDoesNotExist_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var result = await _sut.Handle(
            new SendMessageCommand(Guid.NewGuid(), "hello"),
            CancellationToken.None
        );

        result.Status.Should().Be(ResultStatus.NotFound);
        _conversationRepository.DidNotReceive().AddMessage(Arg.Any<Message>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsNotMember_ReturnsInvalid()
    {
        var userId = Guid.NewGuid();
        var conversation = Conversation.CreateDirect(Guid.NewGuid(), Guid.NewGuid()).Value;

        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var result = await _sut.Handle(
            new SendMessageCommand(conversation.Id, "hello"),
            CancellationToken.None
        );

        result.Status.Should().Be(ResultStatus.Invalid);
        conversation.Messages.Should().BeEmpty();
        _conversationRepository.DidNotReceive().AddMessage(Arg.Any<Message>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMemberSendsMessage_RegistersMessageForInsertAndSaves()
    {
        var userId = Guid.NewGuid();
        var conversation = Conversation.CreateDirect(userId, Guid.NewGuid()).Value;

        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var result = await _sut.Handle(
            new SendMessageCommand(conversation.Id, "  hello  "),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        conversation.Messages.Should().ContainSingle();
        conversation.Messages.Single().Content.Should().Be("hello");

        // The message must be handed to the repository explicitly: it carries a
        // client-generated key while the conversation's Messages collection is not loaded,
        // so relying on change detection would emit an UPDATE instead of an INSERT.
        _conversationRepository
            .Received(1)
            .AddMessage(
                Arg.Is<Message>(message =>
                    message.Id == result.Value
                    && message.ConversationId == conversation.Id
                    && message.SenderId == userId
                    && message.Content == "hello"
                )
            );

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
