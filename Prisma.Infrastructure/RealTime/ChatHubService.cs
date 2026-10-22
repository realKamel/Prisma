using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Application.Features.Chats;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Infrastructure.Persistence;

namespace Prisma.Infrastructure.RealTime;

/// <summary>
/// SignalR bridge. Generic over the concrete hub type so the hub itself stays in the
/// presentation layer while this transport adapter remains in the infrastructure layer.
/// </summary>
public sealed class ChatHubService<THub>(
    IHubContext<THub, IChatHubClient> hubContext,
    AppDbContext dbContext
) : IChatHubService
    where THub : Hub<IChatHubClient>
{
    public Task SendMessageAsync(
        ChatMessageDto message,
        CancellationToken cancellationToken = default
    )
    {
        return hubContext
            .Clients.Group(ChatGroups.Conversation(message.ConversationId))
            .ReceiveMessage(message);
    }

    public Task SendReadReceiptAsync(
        MessageReadReceiptDto receipt,
        CancellationToken cancellationToken = default
    )
    {
        return hubContext
            .Clients.Group(ChatGroups.Conversation(receipt.ConversationId))
            .MessageRead(receipt);
    }

    public Task SendUnreadCountAsync(
        Guid userId,
        UnreadCountDto update,
        CancellationToken cancellationToken = default
    )
    {
        return hubContext.Clients.User(userId.ToString()).UnreadCountUpdated(update);
    }

    public async Task BroadcastPresenceAsync(
        Guid userId,
        bool isOnline,
        CancellationToken cancellationToken = default
    )
    {
        var conversationIds = await dbContext
            .Set<ConversationMember>()
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .Select(member => member.ConversationId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var presence = new PresenceChangedDto(userId, isOnline, DateTimeOffset.UtcNow);

        foreach (var conversationId in conversationIds)
        {
            await hubContext
                .Clients.Group(ChatGroups.Conversation(conversationId))
                .UserPresenceChanged(presence);
        }
    }
}
