using Ardalis.Specification;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Domain.Specifications.Conversations;

public class GetMessagesPageSpec : Specification<Message>
{
    public GetMessagesPageSpec(Guid conversationId, DateTimeOffset? before, int take)
    {
        Query.AsNoTracking().Where(message => message.ConversationId == conversationId);

        if (before.HasValue)
        {
            Query.Where(message => message.SentAt < before.Value);
        }

        Query
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.Id)
            .Take(take);
    }
}
