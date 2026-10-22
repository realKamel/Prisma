using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.ReadModels.Chats;

namespace Prisma.Domain.Repositories;

public interface IConversationRepository
{
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);

    Task<Conversation?> GetByIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    );

    Task<Conversation?> GetDirectConversationAsync(
        Guid userId,
        Guid otherUserId,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsMemberAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<Message?> GetMessageAsync(
        Guid conversationId,
        Guid messageId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Message>> GetMessagesPageAsync(
        Guid conversationId,
        DateTimeOffset? before,
        int take,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ConversationMemberUnreadCount>> GetUnreadCountsAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ConversationSummaryReadModel>> GetUserConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    // Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
