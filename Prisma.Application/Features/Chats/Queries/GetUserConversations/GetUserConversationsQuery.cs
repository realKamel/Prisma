using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Chats.Dtos;

namespace Prisma.Application.Features.Chats.Queries.GetUserConversations;

public sealed record GetUserConversationsQuery
    : IRequest<Result<IReadOnlyList<ConversationSummaryDto>>>;