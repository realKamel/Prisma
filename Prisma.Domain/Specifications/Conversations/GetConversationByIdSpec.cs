using Ardalis.Specification;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Domain.Specifications.Conversations;

public class GetConversationByIdSpec : Specification<Conversation>
{
    public GetConversationByIdSpec(Guid conversationId)
    {
        Query
            .Where(conversation => conversation.Id == conversationId)
            .Include(conversation => conversation.Members);
    }
}
