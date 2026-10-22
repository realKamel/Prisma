namespace Prisma.Domain.ReadModels.Chats;

public sealed record ConversationMemberUnreadCount(Guid UserId, int UnreadCount);