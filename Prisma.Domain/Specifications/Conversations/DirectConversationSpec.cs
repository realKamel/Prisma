using Ardalis.Specification;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Conversations;

public class DirectConversationSpec : Specification<Conversation>
{
    public DirectConversationSpec(Guid userId, Guid otherUserId)
    {
        Query
            .Include(conversation => conversation.Members)
            .Where(conversation => conversation.Type == ConversationType.Direct)
            .Where(conversation =>
                conversation.Members.Count == 2
                && conversation.Members.Any(member => member.UserId == userId)
                && conversation.Members.Any(member => member.UserId == otherUserId)
            );
    }
}
