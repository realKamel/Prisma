using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Chats.Dtos;

namespace Prisma.Application.Features.Chats.Queries.GetConversation;

public sealed record GetConversationQuery(Guid ConversationId)
    : IRequest<Result<ConversationDto>>;