using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Chats.Queries.GetConversation;

internal sealed class GetConversationQueryHandler(
    ICurrentUserService currentUserService,
    IConversationRepository conversationRepository
) : IRequestHandler<GetConversationQuery, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(
        GetConversationQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result<ConversationDto>.Unauthorized();
        }

        var conversation = await conversationRepository.GetByIdAsync(
            request.ConversationId,
            cancellationToken
        );

        if (conversation is null)
        {
            return Result<ConversationDto>.NotFound(
                $"Conversation '{request.ConversationId}' was not found."
            );
        }

        if (!conversation.HasMember(userId.Value))
        {
            return Result<ConversationDto>.Forbidden(
                "You are not a member of this conversation."
            );
        }

        return Result<ConversationDto>.Success(
            new ConversationDto(
                conversation.Id,
                conversation.Type,
                conversation.Title,
                conversation.CreatedAt,
                conversation
                    .Members.Select(member => new ConversationMemberDto(
                        member.UserId,
                        member.Role,
                        member.JoinedAt,
                        member.LastReadMessageId,
                        member.LastReadAt
                    ))
                    .ToList()
            )
        );
    }
}