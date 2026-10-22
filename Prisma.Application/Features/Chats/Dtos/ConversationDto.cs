using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Chats.Dtos;

public sealed record ConversationDto(
    Guid Id,
    ConversationType Type,
    string? Title,
    DateTimeOffset? CreatedAt,
    IReadOnlyCollection<ConversationMemberDto> Members
);
