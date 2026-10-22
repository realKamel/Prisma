using Prisma.Application.Features.Chats.Dtos;

namespace Prisma.Application.Abstractions.Services.Chatting;

/// <summary>
/// Caches unread counters for members that are offline so the badges can be replayed
/// when they reconnect.
/// </summary>
public interface IUnreadBadgeStore
{
    Task SetAsync(
        Guid userId,
        UnreadCountDto update,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyDictionary<Guid, int>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task RemoveAllAsync(Guid userId, CancellationToken cancellationToken = default);
}
