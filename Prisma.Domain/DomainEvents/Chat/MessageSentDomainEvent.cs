using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.Chat;

public sealed record MessageSentDomainEvent(
    Guid ConversationId,
    Guid MessageId,
    Guid SenderId,
    string Content,
    DateTimeOffset SentAt
) : BaseDomainEvent;