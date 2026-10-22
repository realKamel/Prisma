namespace Prisma.Application.Features.Chats.Dtos;

public sealed record TypingIndicatorDto(
    Guid ConversationId,
    Guid UserId,
    bool IsTyping,
    DateTimeOffset OccurredAt
);