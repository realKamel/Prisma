using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Chats.Dtos;

public sealed record ConversationSummaryDto(
    Guid Id,
    ConversationType Type,
    string? Title,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? LastMessageAt,
    string? LastMessageContent,
    Guid? LastMessageSenderId,
    int UnreadCount,
    IReadOnlyCollection<Guid> MemberUserIds
);
