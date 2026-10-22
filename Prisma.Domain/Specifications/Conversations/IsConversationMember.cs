using Ardalis.Specification;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Domain.Specifications.Conversations;

public class IsConversationMember : Specification<ConversationMember>
{
    public IsConversationMember(Guid conversationId, Guid userId)
    {
        Query.Where(member => member.ConversationId == conversationId && member.UserId == userId);
    }
}
