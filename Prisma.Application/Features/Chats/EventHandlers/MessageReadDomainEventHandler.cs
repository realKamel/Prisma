using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Domain.DomainEvents.Chat;

namespace Prisma.Application.Features.Chats.EventHandlers;

/// <summary>
/// Wolverine convention handler: broadcasts seen receipts to active members and clears
/// any pending unread badge for the reader.
/// </summary>
public sealed class MessageReadDomainEventHandler(
    IChatHubService chatHubService,
    IUnreadBadgeStore unreadBadgeStore
)
{
    public async Task Handle(
        MessageReadDomainEvent messageRead,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(messageRead);

        var receipt = new MessageReadReceiptDto(
            messageRead.ConversationId,
            messageRead.UserId,
            messageRead.LastReadMessageId,
            messageRead.ReadAt
        );

        await chatHubService.SendReadReceiptAsync(receipt, cancellationToken);

        await unreadBadgeStore.RemoveAsync(
            messageRead.UserId,
            messageRead.ConversationId,
            cancellationToken
        );
    }
}
