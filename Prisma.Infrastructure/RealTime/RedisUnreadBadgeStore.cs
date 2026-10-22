using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Application.Features.Chats.Dtos;
using StackExchange.Redis;

namespace Prisma.Infrastructure.RealTime;

// Public so Wolverine's codegen can resolve it for event handler dependencies.
public sealed class RedisUnreadBadgeStore(
    [FromKeyedServices("messaging")] IConnectionMultiplexer connectionMultiplexer
) : IUnreadBadgeStore
{
    public async Task SetAsync(
        Guid userId,
        UnreadCountDto update,
        CancellationToken cancellationToken = default
    )
    {
        await connectionMultiplexer
            .GetDatabase()
            .HashSetAsync(
                UnreadKey(userId),
                update.ConversationId.ToString("N"),
                update.UnreadCount
            );
    }

    public async Task RemoveAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        await connectionMultiplexer
            .GetDatabase()
            .HashDeleteAsync(UnreadKey(userId), conversationId.ToString("N"));
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var entries = await connectionMultiplexer.GetDatabase().HashGetAllAsync(UnreadKey(userId));

        var badges = new Dictionary<Guid, int>(entries.Length);

        foreach (var entry in entries)
        {
            if (
                Guid.TryParse(entry.Name.ToString(), out var conversationId)
                && int.TryParse(entry.Value.ToString(), out var unreadCount)
            )
            {
                badges[conversationId] = unreadCount;
            }
        }

        return badges;
    }

    public Task RemoveAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return connectionMultiplexer.GetDatabase().KeyDeleteAsync(UnreadKey(userId));
    }

    private static RedisKey UnreadKey(Guid userId) => $"chat:unread:{userId:N}";
}
