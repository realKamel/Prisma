using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.API.Features.Chats.Requests;
using Prisma.Application.Features.Chats.Commands.CreateDirectConversation;
using Prisma.Application.Features.Chats.Commands.CreateGroupConversation;
using Prisma.Application.Features.Chats.Commands.MarkAsRead;
using Prisma.Application.Features.Chats.Commands.SendMessage;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Application.Features.Chats.Queries.GetConversation;
using Prisma.Application.Features.Chats.Queries.GetConversationMessages;
using Prisma.Application.Features.Chats.Queries.GetUserConversations;

namespace Prisma.API.Features.Chats;

[Authorize]
public sealed class ChatsController(ISender mediator) : ApiController
{
    [HttpGet]
    public async Task<Result<IReadOnlyList<ConversationSummaryDto>>> GetConversations(
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(new GetUserConversationsQuery(), cancellationToken);
    }

    [HttpPost("direct")]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result<Guid>> CreateDirectConversation(
        [FromBody] CreateDirectConversationRequest request,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new CreateDirectConversationCommand(request.OtherUserId),
            cancellationToken
        );
    }

    [HttpPost("group")]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result<Guid>> CreateGroupConversation(
        [FromBody] CreateGroupConversationRequest request,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new CreateGroupConversationCommand(request.Title, request.MemberIds),
            cancellationToken
        );
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<Result<ConversationDto>> GetConversation(
        Guid conversationId,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new GetConversationQuery(conversationId),
            cancellationToken
        );
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<Result<CursorPagedResult<ChatMessageDto>>> GetMessages(
        Guid conversationId,
        [FromQuery] DateTimeOffset? before,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default
    )
    {
        return await mediator.Send(
            new GetConversationMessagesQuery(conversationId, before, pageSize),
            cancellationToken
        );
    }

    [HttpPost("{conversationId:guid}/messages")]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result<Guid>> SendMessage(
        Guid conversationId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new SendMessageCommand(conversationId, request.Content),
            cancellationToken
        );
    }

    [HttpPost("{conversationId:guid}/read")]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result> MarkAsRead(
        Guid conversationId,
        [FromBody] MarkAsReadRequest request,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new MarkAsReadCommand(conversationId, request.LastMessageId),
            cancellationToken
        );
    }
}