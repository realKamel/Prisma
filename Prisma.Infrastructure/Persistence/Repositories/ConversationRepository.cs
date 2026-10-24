using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.ReadModels.Chats;
using Prisma.Domain.Repositories;

namespace Prisma.Infrastructure.Persistence.Repositories;

// Public so Wolverine's codegen can resolve it for event handler dependencies
// without falling back to service location.
public sealed class ConversationRepository(AppDbContext dbContext) : IConversationRepository
{
    public async Task AddAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.Set<Conversation>().Add(conversation);
    }

    /// <summary>
    /// Tracks a message created by <see cref="Conversation.AddMessage"/> as Added.
    /// Messages are immutable and carry a client-generated Guid key, so once the aggregate
    /// hands one back it must be inserted explicitly; relying on change detection over the
    /// (not loaded) Messages collection would emit an UPDATE against a row that does not
    /// exist yet and fail with a DbUpdateConcurrencyException.
    /// </summary>
    public void AddMessage(Message message)
    {
        dbContext.Set<Message>().Add(message);
    }

    public Task<Conversation?> GetByIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Set<Conversation>()
            .Include(conversation => conversation.Members)
            .FirstOrDefaultAsync(
                conversation => conversation.Id == conversationId,
                cancellationToken
            );
    }

    public Task<Conversation?> GetDirectConversationAsync(
        Guid userId,
        Guid otherUserId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Set<Conversation>()
            .Include(conversation => conversation.Members)
            .Where(conversation => conversation.Type == ConversationType.Direct)
            .Where(conversation =>
                conversation.Members.Count == 2
                && conversation.Members.Any(member => member.UserId == userId)
                && conversation.Members.Any(member => member.UserId == otherUserId)
            )
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> IsMemberAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Set<ConversationMember>()
            .AnyAsync(
                member => member.ConversationId == conversationId && member.UserId == userId,
                cancellationToken
            );
    }

    public Task<Message?> GetMessageAsync(
        Guid conversationId,
        Guid messageId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Set<Message>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                message => message.ConversationId == conversationId && message.Id == messageId,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Message>> GetMessagesPageAsync(
        Guid conversationId,
        DateTimeOffset? before,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .Set<Message>()
            .AsNoTracking()
            .Where(message => message.ConversationId == conversationId);

        if (before.HasValue)
        {
            query = query.Where(message => message.SentAt < before.Value);
        }

        return await query
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationMemberUnreadCount>> GetUnreadCountsAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        var messages = dbContext.Set<Message>().AsNoTracking();

        return await dbContext
            .Set<ConversationMember>()
            .AsNoTracking()
            .Where(member => member.ConversationId == conversationId)
            .Select(member => new ConversationMemberUnreadCount(
                member.UserId,
                messages.Count(message =>
                    message.ConversationId == conversationId
                    && message.SentAt > (member.LastReadAt ?? DateTimeOffset.MinValue)
                )
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationSummaryReadModel>> GetUserConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var conversations = dbContext.Set<Conversation>().AsNoTracking();
        var messages = dbContext.Set<Message>().AsNoTracking();

        var summaries = await (
            from member in dbContext.Set<ConversationMember>().AsNoTracking()
            join conversation in conversations on member.ConversationId equals conversation.Id
            where member.UserId == userId
            orderby conversation.LastMessageAt ?? conversation.CreatedAt descending
            select new ConversationSummaryReadModel(
                conversation.Id,
                conversation.Type,
                conversation.Title,
                conversation.CreatedAt,
                conversation.LastMessageAt,
                messages
                    .Where(message => message.ConversationId == conversation.Id)
                    .OrderByDescending(message => message.SentAt)
                    .Select(message => message.Content)
                    .FirstOrDefault(),
                messages
                    .Where(message => message.ConversationId == conversation.Id)
                    .OrderByDescending(message => message.SentAt)
                    .Select(message => (Guid?)message.SenderId)
                    .FirstOrDefault(),
                messages.Count(message =>
                    message.ConversationId == conversation.Id
                    && message.SentAt > (member.LastReadAt ?? DateTimeOffset.MinValue)
                )
            )
        ).ToListAsync(cancellationToken);

        if (summaries.Count == 0)
        {
            return summaries;
        }

        var conversationIds = summaries.Select(summary => summary.ConversationId).ToList();

        var membersByConversation = (
            await dbContext
                .Set<ConversationMember>()
                .AsNoTracking()
                .Where(member => conversationIds.Contains(member.ConversationId))
                .Select(member => new { member.ConversationId, member.UserId })
                .ToListAsync(cancellationToken)
        )
            .GroupBy(member => member.ConversationId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<Guid>)group.Select(member => member.UserId).ToList()
            );

        return summaries
            .Select(summary =>
                summary with
                {
                    MemberUserIds = membersByConversation.GetValueOrDefault(
                        summary.ConversationId,
                        []
                    ),
                }
            )
            .ToList();
    }
}
