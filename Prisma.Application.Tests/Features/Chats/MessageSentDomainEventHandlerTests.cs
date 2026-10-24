using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Application.Features.Chats.EventHandlers;
using Prisma.Domain.DomainEvents.Chat;
using Prisma.Domain.ReadModels.Chats;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Tests.Features.Chats;

public class MessageSentDomainEventHandlerTests
{
    private readonly IConversationRepository _conversationRepository =
        Substitute.For<IConversationRepository>();

    private readonly IChatHubService _chatHubService = Substitute.For<IChatHubService>();

    private readonly IChatPresenceTracker _presenceTracker = Substitute.For<IChatPresenceTracker>();

    private readonly IUnreadBadgeStore _unreadBadgeStore = Substitute.For<IUnreadBadgeStore>();
    private readonly MessageSentDomainEventHandler _sut;

    public MessageSentDomainEventHandlerTests()
    {
        _sut = new MessageSentDomainEventHandler(
            _conversationRepository,
            _chatHubService,
            _presenceTracker,
            _unreadBadgeStore,
            NullLogger<MessageSentDomainEventHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_BroadcastsMessageAndPushesBadgesToOnlineAndOfflineMembers()
    {
        var conversationId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var onlineUserId = Guid.NewGuid();
        var offlineUserId = Guid.NewGuid();
        var sentAt = DateTimeOffset.UtcNow;

        var messageSent = new MessageSentDomainEvent(
            conversationId,
            Guid.NewGuid(),
            senderId,
            "hello",
            sentAt
        );

        _conversationRepository
            .GetUnreadCountsAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(
                new List<ConversationMemberUnreadCount>
                {
                    new(senderId, 0),
                    new(onlineUserId, 4),
                    new(offlineUserId, 7),
                }
            );

        _presenceTracker
            .GetOnlineUsersAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { onlineUserId });

        await _sut.Handle(messageSent, CancellationToken.None);

        await _chatHubService
            .Received(1)
            .SendMessageAsync(
                Arg.Is<ChatMessageDto>(message =>
                    message.Id == messageSent.MessageId
                    && message.ConversationId == conversationId
                    && message.SenderId == senderId
                    && message.Content == "hello"
                ),
                Arg.Any<CancellationToken>()
            );

        await _chatHubService
            .Received(1)
            .SendUnreadCountAsync(
                onlineUserId,
                Arg.Is<UnreadCountDto>(update =>
                    update.ConversationId == conversationId && update.UnreadCount == 4
                ),
                Arg.Any<CancellationToken>()
            );

        await _unreadBadgeStore
            .Received(1)
            .SetAsync(
                offlineUserId,
                Arg.Is<UnreadCountDto>(update =>
                    update.ConversationId == conversationId && update.UnreadCount == 7
                ),
                Arg.Any<CancellationToken>()
            );

        await _chatHubService
            .DidNotReceive()
            .SendUnreadCountAsync(
                senderId,
                Arg.Any<UnreadCountDto>(),
                Arg.Any<CancellationToken>()
            );

        await _unreadBadgeStore
            .DidNotReceive()
            .SetAsync(senderId, Arg.Any<UnreadCountDto>(), Arg.Any<CancellationToken>());

        messageSent.Should().NotBeNull();
    }
}
