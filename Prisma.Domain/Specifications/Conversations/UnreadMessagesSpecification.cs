using Ardalis.Specification;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Domain.Specifications.Chat;

public sealed class UnreadMessagesSpecification : Specification<Conversation, int>
{
    public UnreadMessagesSpecification(Guid userId)
    {
        Query.Where(c => c.Members.Any(m =>
            m.UserId == userId
            && c.Messages.Any(msg =>
                msg.SenderId != userId
                && (m.LastReadAt == null || msg.SentAt > m.LastReadAt))));
    }
}