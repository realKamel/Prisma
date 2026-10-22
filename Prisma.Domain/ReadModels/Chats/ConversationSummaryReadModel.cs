using Prisma.Domain.Enums;

namespace Prisma.Domain.ReadModels.Chats;

public sealed record ConversationSummaryReadModel(
    Guid ConversationId,
    ConversationType Type,
    string? Title,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? LastMessageAt,
    string? LastMessageContent,
    Guid? LastMessageSenderId,
    int UnreadCount
)
{
    public IReadOnlyCollection<Guid> MemberUserIds { get; init; } = [];
}
