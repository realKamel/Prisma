using Ardalis.Specification;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Domain.Specifications.Conversations;

public class GetMessageSpec : Specification<Message>
{
    public GetMessageSpec(Guid conversationId, Guid messageId)
    {
        Query
            .Where(message => message.ConversationId == conversationId && message.Id == messageId)
            .AsNoTracking();
    }
}
