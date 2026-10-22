using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Chats.Commands.CreateDirectConversation;

public sealed record CreateDirectConversationCommand(Guid OtherUserId) : IRequest<Result<Guid>>;