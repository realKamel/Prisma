using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Chats.Dtos;

public sealed record ConversationMemberDto(
    Guid UserId,
    ConversationRole Role,
    DateTimeOffset JoinedAt,
    Guid? LastReadMessageId,
    DateTimeOffset? LastReadAt
);