namespace Prisma.Application.Features.Chats.Dtos;

public sealed record MessageReadReceiptDto(
    Guid ConversationId,
    Guid UserId,
    Guid LastReadMessageId,
    DateTimeOffset ReadAt
);