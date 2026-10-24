using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Services.Chatting;
using StackExchange.Redis;

namespace Prisma.Infrastructure.RealTime;

// Public so Wolverine's codegen can resolve it for event handler dependencies.
public sealed class RedisChatPresenceTracker(
    [FromKeyedServices("messaging")] IConnectionMultiplexer connectionMultiplexer
) : IChatPresenceTracker
{
    private static readonly TimeSpan ConnectionSetTtl = TimeSpan.FromDays(1);

    public async Task TrackConnectionAsync(
        Guid userId,
        string connectionId,
        CancellationToken cancellationToken = default
    )
    {
        var database = connectionMultiplexer.GetDatabase();
        var key = UserConnectionsKey(userId);

        await database.SetAddAsync(key, connectionId);
        await database.KeyExpireAsync(key, ConnectionSetTtl);
    }

    public async Task<bool> DisconnectConnectionAsync(
        Guid userId,
        string connectionId,
        CancellationToken cancellationToken = default
    )
    {
        var database = connectionMultiplexer.GetDatabase();
        var key = UserConnectionsKey(userId);

        await database.SetRemoveAsync(key, connectionId);

        var remainingConnections = await database.SetLengthAsync(key);

        if (remainingConnections > 0)
        {
            return false;
        }

        await database.KeyDeleteAsync(key);
        return true;
    }

    public async Task<bool> IsOnlineAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var length = await connectionMultiplexer
            .GetDatabase()
            .SetLengthAsync(UserConnectionsKey(userId));

        return length > 0;
    }

    public async Task<IReadOnlyCollection<Guid>> GetOnlineUsersAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var database = connectionMultiplexer.GetDatabase();
        var distinctUserIds = userIds.Distinct().ToList();
        var lengths = await Task.WhenAll(
            distinctUserIds.Select(userId => database.SetLengthAsync(UserConnectionsKey(userId)))
        );

        var onlineUsers = new List<Guid>(distinctUserIds.Count);

        for (var index = 0; index < distinctUserIds.Count; index++)
        {
            if (lengths[index] > 0)
            {
                onlineUsers.Add(distinctUserIds[index]);
            }
        }

        return onlineUsers;
    }

    private static RedisKey UserConnectionsKey(Guid userId) => $"chat:presence:{userId:N}";
}
