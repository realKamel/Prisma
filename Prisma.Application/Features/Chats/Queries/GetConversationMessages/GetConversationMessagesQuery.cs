using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Chats.Dtos;

namespace Prisma.Application.Features.Chats.Queries.GetConversationMessages;

/// <param name="Before">
/// Timestamp cursor. When supplied, only messages older than this instant are returned.
/// </param>
public sealed record GetConversationMessagesQuery(
    Guid ConversationId,
    DateTimeOffset? Before = null,
    int PageSize = 30
) : IRequest<Result<CursorPagedResult<ChatMessageDto>>>;