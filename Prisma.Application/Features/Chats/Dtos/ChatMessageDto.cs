namespace Prisma.Application.Features.Chats.Dtos;

public sealed record ChatMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    DateTimeOffset SentAt
);