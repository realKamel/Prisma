using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Chats.Commands.MarkAsRead;

public sealed record MarkAsReadCommand(Guid ConversationId, Guid LastMessageId)
    : IRequest<Result>;