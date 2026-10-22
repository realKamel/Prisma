namespace Prisma.Application.Abstractions.Services.Chatting;

public interface IChatPresenceTracker
{
    Task TrackConnectionAsync(
        Guid userId,
        string connectionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes a connection and returns <c>true</c> when the user has no remaining connections.
    /// </summary>
    Task<bool> DisconnectConnectionAsync(
        Guid userId,
        string connectionId,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsOnlineAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guid>> GetOnlineUsersAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default
    );
}
