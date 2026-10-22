using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.ChatAggregate;

public sealed class Message : BaseEntity<Guid>
{
    private Message() { }

    private Message(
        Guid id,
        Guid conversationId,
        Guid senderId,
        string content,
        DateTimeOffset sentAt
    )
    {
        Id = id;
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
        SentAt = sentAt;
    }

    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset SentAt { get; private set; }

    internal static Message Create(
        Guid conversationId,
        Guid senderId,
        string content,
        DateTimeOffset sentAt
    ) => new(Guid.CreateVersion7(), conversationId, senderId, content, sentAt);
}
