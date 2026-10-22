using Prisma.Application.Features.Chats.Dtos;

namespace Prisma.Application.Abstractions.Services.Chatting;

/// <summary>
/// Strongly typed SignalR client contract. Implemented by the presentation layer hub.
/// </summary>
public interface IChatHubClient
{
    Task ReceiveMessage(ChatMessageDto message);
    Task MessageRead(MessageReadReceiptDto receipt);
    Task UserTyping(TypingIndicatorDto indicator);
    Task UnreadCountUpdated(UnreadCountDto update);
    Task UserPresenceChanged(PresenceChangedDto presence);
}
