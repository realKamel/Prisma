using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Chats.Dtos;

namespace Prisma.Application.Features.Chats.Queries.GetConversation;

public sealed record GetConversationQuery(Guid ConversationId) : IQuery<Result<ConversationDto>>;
