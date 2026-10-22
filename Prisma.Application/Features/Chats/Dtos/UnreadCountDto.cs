namespace Prisma.Application.Features.Chats.Dtos;

public sealed record UnreadCountDto(Guid ConversationId, int UnreadCount);