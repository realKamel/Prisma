using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Chats.Queries.GetConversationMessages;

internal sealed class GetConversationMessagesQueryHandler(
    ICurrentUserService currentUserService,
    IConversationRepository conversationRepository
) : IRequestHandler<GetConversationMessagesQuery, Result<CursorPagedResult<ChatMessageDto>>>
{
    public async Task<Result<CursorPagedResult<ChatMessageDto>>> Handle(
        GetConversationMessagesQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result<CursorPagedResult<ChatMessageDto>>.Unauthorized();
        }

        var isMember = await conversationRepository.IsMemberAsync(
            request.ConversationId,
            userId.Value,
            cancellationToken
        );

        if (!isMember)
        {
            return Result<CursorPagedResult<ChatMessageDto>>.Forbidden(
                "You are not a member of this conversation."
            );
        }

        var page = await conversationRepository.GetMessagesPageAsync(
            request.ConversationId,
            request.Before,
            request.PageSize + 1,
            cancellationToken
        );

        var hasMore = page.Count > request.PageSize;

        var items = page
            .Take(request.PageSize)
            .Select(message => new ChatMessageDto(
                message.Id,
                message.ConversationId,
                message.SenderId,
                message.Content,
                message.SentAt
            ))
            .ToList();

        var nextCursor = hasMore && items.Count > 0 ? items[^1].SentAt : (DateTimeOffset?)null;

        return Result<CursorPagedResult<ChatMessageDto>>.Success(
            new CursorPagedResult<ChatMessageDto>(items, nextCursor, hasMore)
        );
    }
}