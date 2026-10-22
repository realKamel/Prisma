using Microsoft.Extensions.Logging;
using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Domain.DomainEvents.Chat;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Chats.EventHandlers;

/// <summary>
/// Wolverine convention handler: broadcasts new messages in real time and pushes
/// unread badge updates for every other conversation member.
/// </summary>
public sealed partial class MessageSentDomainEventHandler(
    IConversationRepository conversationRepository,
    IChatHubService chatHubService,
    IChatPresenceTracker presenceTracker,
    IUnreadBadgeStore unreadBadgeStore,
    ILogger<MessageSentDomainEventHandler> logger
)
{
    public async Task Handle(
        MessageSentDomainEvent messageSent,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(messageSent);

        var message = new ChatMessageDto(
            messageSent.MessageId,
            messageSent.ConversationId,
            messageSent.SenderId,
            messageSent.Content,
            messageSent.SentAt
        );

        await chatHubService.SendMessageAsync(message, cancellationToken);

        var unreadCounts = await conversationRepository.GetUnreadCountsAsync(
            messageSent.ConversationId,
            cancellationToken
        );

        var recipients = unreadCounts
            .Where(member => member.UserId != messageSent.SenderId)
            .ToList();

        if (recipients.Count == 0)
        {
            return;
        }

        var onlineUsers = await presenceTracker.GetOnlineUsersAsync(
            recipients.Select(recipient => recipient.UserId),
            cancellationToken
        );

        var onlineUserIds = onlineUsers.ToHashSet();

        foreach (var recipient in recipients)
        {
            var update = new UnreadCountDto(messageSent.ConversationId, recipient.UnreadCount);

            if (onlineUserIds.Contains(recipient.UserId))
            {
                await chatHubService.SendUnreadCountAsync(
                    recipient.UserId,
                    update,
                    cancellationToken
                );
            }
            else
            {
                await unreadBadgeStore.SetAsync(recipient.UserId, update, cancellationToken);
            }
        }

        LogBroadcastMessageMessageIdToConversationConversationIdWithRecipientCountRecipients(
            messageSent.MessageId,
            messageSent.ConversationId,
            recipients.Count
        );
    }

    [LoggerMessage(
        LogLevel.Debug,
        "Broadcast message {MessageId} to conversation {ConversationId} with {RecipientCount} recipients"
    )]
    partial void LogBroadcastMessageMessageIdToConversationConversationIdWithRecipientCountRecipients(
        Guid messageId,
        Guid conversationId,
        int recipientCount
    );
}
