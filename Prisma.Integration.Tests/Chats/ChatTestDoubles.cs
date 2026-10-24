using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Application.Features.Chats.Dtos;

namespace Prisma.Integration.Tests.Chats;

/// <summary>
/// Records the broadcasts that <c>MessageSentDomainEventHandler</c> / <c>MessageReadDomainEventHandler</c>
/// issue through <see cref="IChatHubService"/>, so a test can prove the domain event was actually
/// published and handled in process (and not only that the row was persisted).
/// </summary>
internal sealed class RecordingChatHubService : IChatHubService
{
    private readonly Lock _gate = new();
    private readonly List<ChatMessageDto> _messages = [];
    private readonly List<MessageReadReceiptDto> _readReceipts = [];
    private readonly List<UnreadCountDto> _unreadCounts = [];

    public IReadOnlyList<ChatMessageDto> Messages
    {
        get
        {
            lock (_gate)
            {
                return _messages.ToArray();
            }
        }
    }

    public IReadOnlyList<MessageReadReceiptDto> ReadReceipts
    {
        get
        {
            lock (_gate)
            {
                return _readReceipts.ToArray();
            }
        }
    }

    public IReadOnlyList<UnreadCountDto> UnreadCounts
    {
        get
        {
            lock (_gate)
            {
                return _unreadCounts.ToArray();
            }
        }
    }

    public Task SendMessageAsync(
        ChatMessageDto message,
        CancellationToken cancellationToken = default
    )
    {
        lock (_gate)
        {
            _messages.Add(message);
        }

        return Task.CompletedTask;
    }

    public Task SendReadReceiptAsync(
        MessageReadReceiptDto receipt,
        CancellationToken cancellationToken = default
    )
    {
        lock (_gate)
        {
            _readReceipts.Add(receipt);
        }

        return Task.CompletedTask;
    }

    public Task SendUnreadCountAsync(
        Guid userId,
        UnreadCountDto update,
        CancellationToken cancellationToken = default
    )
    {
        lock (_gate)
        {
            _unreadCounts.Add(update);
        }

        return Task.CompletedTask;
    }

    public Task BroadcastPresenceAsync(
        Guid userId,
        bool isOnline,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;

    public async Task<ChatMessageDto> WaitForMessageAsync(Guid messageId, TimeSpan timeout)
    {
        using var timeoutSource = new CancellationTokenSource(timeout);

        while (!timeoutSource.IsCancellationRequested)
        {
            lock (_gate)
            {
                var match = _messages.Find(message => message.Id == messageId);

                if (match is not null)
                {
                    return match;
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25), timeoutSource.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        throw new TimeoutException(
            $"Message '{messageId}' was not broadcast through IChatHubService within {timeout}."
        );
    }
}

/// <summary>Every chat member is treated as offline, so unread counts are cached instead of pushed.</summary>
internal sealed class OfflineChatPresenceTracker : IChatPresenceTracker
{
    public Task TrackConnectionAsync(
        Guid userId,
        string connectionId,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;

    public Task<bool> DisconnectConnectionAsync(
        Guid userId,
        string connectionId,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(false);

    public Task<bool> IsOnlineAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task<IReadOnlyCollection<Guid>> GetOnlineUsersAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default
    ) => Task.FromResult<IReadOnlyCollection<Guid>>([]);
}

internal sealed class InMemoryUnreadBadgeStore : IUnreadBadgeStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<Guid, Dictionary<Guid, int>> _badges = [];

    public Task SetAsync(
        Guid userId,
        UnreadCountDto update,
        CancellationToken cancellationToken = default
    )
    {
        lock (_gate)
        {
            if (!_badges.TryGetValue(userId, out var perUser))
            {
                perUser = [];
                _badges[userId] = perUser;
            }

            perUser[update.ConversationId] = update.UnreadCount;
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        lock (_gate)
        {
            if (_badges.TryGetValue(userId, out var perUser))
            {
                perUser.Remove(conversationId);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<Guid, int>> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        lock (_gate)
        {
            IReadOnlyDictionary<Guid, int> snapshot = _badges.TryGetValue(userId, out var perUser)
                ? new Dictionary<Guid, int>(perUser)
                : new Dictionary<Guid, int>();

            return Task.FromResult(snapshot);
        }
    }

    public Task RemoveAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _badges.Remove(userId);
        }

        return Task.CompletedTask;
    }
}
