namespace Prisma.API.Features.Chats.Requests;

public sealed record CreateDirectConversationRequest(Guid OtherUserId);