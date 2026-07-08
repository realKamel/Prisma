using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence;

public sealed class PostgresChatHistoryProvider : ChatHistoryProvider
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ProviderSessionState<State> _sessionState;
    private readonly JsonSerializerOptions _jsonOptions;

    // Optional: cap how much history gets loaded per turn, so RAG conversations
    // don't grow the prompt unboundedly. null = load everything.
    private readonly int? _maxMessagesToLoad;

    public PostgresChatHistoryProvider(
        IDbContextFactory<AppDbContext> dbContextFactory,
        JsonSerializerOptions? jsonOptions = null,
        int? maxMessagesToLoad = null,
        string? stateKey = null)
    {
        _dbContextFactory = dbContextFactory;
        _jsonOptions = jsonOptions ?? AIJsonUtilities.DefaultOptions;
        _maxMessagesToLoad = maxMessagesToLoad;

        // Only thing stored in AgentSession state is the DB key (SessionId),
        // never the messages themselves — those live in Postgres.
        _sessionState = new ProviderSessionState<State>(
            stateInitializer: _ => new State { SessionId = Guid.NewGuid() },
            stateKey: stateKey ?? this.GetType().Name);
    }

    public string StateKey => _sessionState.StateKey;

    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        var state = _sessionState.GetOrInitializeState(context.Session);

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<ChatHistoryMessageEntity> query = db.Set<ChatHistoryMessageEntity>()
            .AsNoTracking()
            .Where(m => m.SessionId == state.SessionId)
            .OrderByDescending(m => m.Sequence);

        if (_maxMessagesToLoad is int limit)
        {
            query = query.Take(limit);
        }

        var rows = await query.ToListAsync(cancellationToken);
        rows.Reverse(); // back to chronological order

        return rows.Select(r =>
            JsonSerializer.Deserialize<ChatMessage>(r.MessageJson, _jsonOptions)!);
    }

    protected override async ValueTask StoreChatHistoryAsync(
        InvokedContext context,
        CancellationToken cancellationToken = default)
    {
        var state = _sessionState.GetOrInitializeState(context.Session);

        var allNewMessages = context.RequestMessages
            .Concat(context.ResponseMessages ?? [])
            .ToList();

        if (allNewMessages.Count == 0)
        {
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Get the current max sequence for this session so appends are ordered correctly
        // even if this provider instance is used concurrently across requests.
        var nextSequence = await db.Set<ChatHistoryMessageEntity>()
            .Where(m => m.SessionId == state.SessionId)
            .Select(m => (int?)m.Sequence)
            .MaxAsync(cancellationToken) ?? -1;
        nextSequence++;

        var now = DateTimeOffset.UtcNow;

        var entities = allNewMessages.Select((message, i) => new ChatHistoryMessageEntity
        {
            Id = Guid.NewGuid(),
            SessionId = state.SessionId,
            Sequence = nextSequence + i,
            Role = message.Role.Value,
            MessageJson = JsonSerializer.Serialize(message, _jsonOptions),
            CreatedAtUtc = now
        });

        db.Set<ChatHistoryMessageEntity>().AddRange(entities);

        // Persist the (possibly newly-created) SessionId back onto the AgentSession.
        _sessionState.SaveState(context.Session, state);

        await db.SaveChangesAsync(cancellationToken);
    }

    public sealed class State
    {
        [JsonPropertyName("sessionId")] public Guid SessionId { get; set; }
    }
}