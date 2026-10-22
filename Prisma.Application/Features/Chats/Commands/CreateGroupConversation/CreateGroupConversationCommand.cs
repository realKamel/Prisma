using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Chats.Commands.CreateGroupConversation;

public sealed record CreateGroupConversationCommand(
    string Title,
    IReadOnlyCollection<Guid> MemberIds
) : IRequest<Result<Guid>>;