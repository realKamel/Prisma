using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.Chat;

public sealed record MessageReadDomainEvent(
    Guid ConversationId,
    Guid UserId,
    Guid LastReadMessageId,
    DateTimeOffset ReadAt
) : BaseDomainEvent;