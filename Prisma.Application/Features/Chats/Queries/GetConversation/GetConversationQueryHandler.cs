using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Persistence;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Application.Features.Chats.Queries.GetConversation;

internal sealed class GetConversationQueryHandler(
    ICurrentUserService currentUserService,
    IReadonlyAppDbContext appDbContext
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

        var query = appDbContext.Entity<Conversation, Guid>();

        var conversation = await query.FirstOrDefaultAsync(
            x => request.ConversationId == x.Id,
            cancellationToken
        );

        // var conversation2 = await conversationRepository.GetByIdAsync(
        //     request.ConversationId,
        //     cancellationToken
        // );

        if (conversation is null)
        {
            return Result<ConversationDto>.NotFound(
                $"Conversation '{request.ConversationId}' was not found."
            );
        }

        if (!conversation.HasMember(userId.Value))
        {
            return Result<ConversationDto>.Forbidden("You are not a member of this conversation.");
        }

        return Result<ConversationDto>.Success(
            new ConversationDto(
                conversation.Id,
                conversation.Type,
                conversation.Title,
                conversation.CreatedAt,
                [
                    .. conversation.Members.Select(member => new ConversationMemberDto(
                        member.UserId,
                        member.Role,
                        member.JoinedAt,
                        member.LastReadMessageId,
                        member.LastReadAt
                    )),
                ]
            )
        );
    }
}
