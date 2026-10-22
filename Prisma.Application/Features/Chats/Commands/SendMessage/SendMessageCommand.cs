using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Chats.Commands.SendMessage;

public sealed record SendMessageCommand(Guid ConversationId, string Content)
    : IRequest<Result<Guid>>;