using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Application.Features.Chats;
using Prisma.Application.Features.Chats.Commands.MarkAsRead;
using Prisma.Application.Features.Chats.Commands.SendMessage;
using Prisma.Application.Features.Chats.Dtos;
using Prisma.Domain.Repositories;

namespace Prisma.API.Features.Chats;

[Authorize]
public sealed class ChatHub(
    ISender mediator,
    IConversationRepository conversationRepository,
    IChatHubService chatHubService,
    IChatPresenceTracker presenceTracker,
    IUnreadBadgeStore unreadBadgeStore
) : Hub<IChatHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();

        await presenceTracker.TrackConnectionAsync(
            userId,
            Context.ConnectionId,
            Context.ConnectionAborted
        );

        var pendingBadges = await unreadBadgeStore.GetAsync(userId, Context.ConnectionAborted);

        foreach (var badge in pendingBadges)
        {
            await Clients.Caller.UnreadCountUpdated(new UnreadCountDto(badge.Key, badge.Value));
        }

        if (pendingBadges.Count > 0)
        {
            await unreadBadgeStore.RemoveAllAsync(userId, Context.ConnectionAborted);
        }

        await chatHubService.BroadcastPresenceAsync(userId, true, Context.ConnectionAborted);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TryGetCurrentUserId(out var userId))
        {
            var wentOffline = await presenceTracker.DisconnectConnectionAsync(
                userId,
                Context.ConnectionId
            );

            if (wentOffline)
            {
                await chatHubService.BroadcastPresenceAsync(userId, false);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var userId = GetCurrentUserId();

        await EnsureMembershipAsync(conversationId, userId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            ChatGroups.Conversation(conversationId),
            Context.ConnectionAborted
        );
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        var userId = GetCurrentUserId();

        await EnsureMembershipAsync(conversationId, userId);

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            ChatGroups.Conversation(conversationId),
            Context.ConnectionAborted
        );
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var userId = GetCurrentUserId();

        await EnsureMembershipAsync(conversationId, userId);

        var result = await mediator.Send(
            new SendMessageCommand(conversationId, content),
            Context.ConnectionAborted
        );

        if (!result.IsSuccess)
        {
            throw new HubException(BuildErrorMessage(result));
        }
    }

    public async Task MarkAsRead(Guid conversationId, Guid lastMessageId)
    {
        var userId = GetCurrentUserId();

        await EnsureMembershipAsync(conversationId, userId);

        var result = await mediator.Send(
            new MarkAsReadCommand(conversationId, lastMessageId),
            Context.ConnectionAborted
        );

        if (!result.IsSuccess)
        {
            throw new HubException(BuildErrorMessage(result));
        }
    }

    public async Task SendTypingIndicator(Guid conversationId, bool isTyping)
    {
        var userId = GetCurrentUserId();

        await EnsureMembershipAsync(conversationId, userId);

        await Clients
            .OthersInGroup(ChatGroups.Conversation(conversationId))
            .UserTyping(
                new TypingIndicatorDto(conversationId, userId, isTyping, DateTimeOffset.UtcNow)
            );
    }

    private async Task EnsureMembershipAsync(Guid conversationId, Guid userId)
    {
        var isMember = await conversationRepository.IsMemberAsync(
            conversationId,
            userId,
            Context.ConnectionAborted
        );

        if (!isMember)
        {
            throw new HubException("You are not an active member of this conversation.");
        }
    }

    private Guid GetCurrentUserId()
    {
        return TryGetCurrentUserId(out var userId)
            ? userId
            : throw new HubException("The connection is not authenticated.");
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var value =
            Context.UserIdentifier
            ?? Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out userId);
    }

    private static string BuildErrorMessage(Ardalis.Result.IResult result)
    {
        var errors = result.Errors.Any()
            ? result.Errors
            : result.ValidationErrors.Select(error => error.ErrorMessage);

        return errors.Any() ? string.Join(" ", errors) : "The chat operation failed.";
    }
}
