namespace Prisma.API.Features.Chats.Requests;

public sealed record CreateGroupConversationRequest(
    string Title,
    IReadOnlyCollection<Guid> MemberIds
);