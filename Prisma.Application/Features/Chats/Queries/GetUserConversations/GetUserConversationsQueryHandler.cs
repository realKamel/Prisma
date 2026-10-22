using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Chats.Queries.GetUserConversations;

internal sealed class GetUserConversationsQueryHandler(
    ICurrentUserService currentUserService,
    IConversationRepository conversationRepository
) : IRequestHandler<GetUserConversationsQuery, Result<IReadOnlyList<ConversationSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<ConversationSummaryDto>>> Handle(
        GetUserConversationsQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result<IReadOnlyList<ConversationSummaryDto>>.Unauthorized();
        }

        var conversations = await conversationRepository.GetUserConversationsAsync(
            userId.Value,
            cancellationToken
        );

        IReadOnlyList<ConversationSummaryDto> result = conversations
            .Select(conversation => new ConversationSummaryDto(
                conversation.ConversationId,
                conversation.Type,
                conversation.Title,
                conversation.CreatedAt,
                conversation.LastMessageAt,
                conversation.LastMessageContent,
                conversation.LastMessageSenderId,
                conversation.UnreadCount,
                conversation.MemberUserIds
            ))
            .ToList();

        return Result<IReadOnlyList<ConversationSummaryDto>>.Success(result);
    }
}