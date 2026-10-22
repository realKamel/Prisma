using Prisma.Application.Features.Chats.Dtos;

namespace Prisma.Application.Abstractions.Services.Chatting;

/// <summary>
/// Transport-agnostic bridge used by application event handlers to broadcast
/// chat updates without depending on SignalR.
/// </summary>
public interface IChatHubService
{
    Task SendMessageAsync(ChatMessageDto message, CancellationToken cancellationToken = default);

    Task SendReadReceiptAsync(
        MessageReadReceiptDto receipt,
        CancellationToken cancellationToken = default
    );

    Task SendUnreadCountAsync(
        Guid userId,
        UnreadCountDto update,
        CancellationToken cancellationToken = default
    );

    Task BroadcastPresenceAsync(
        Guid userId,
        bool isOnline,
        CancellationToken cancellationToken = default
    );
}
