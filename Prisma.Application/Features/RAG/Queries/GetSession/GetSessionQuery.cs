using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.RAG.Queries.GetSession;

public record GetDetailedRagSessionQuery(Guid Id) : IRequest<Result<GetDetailedRagSessionQueryResponse>>;

public record GetDetailedRagSessionQueryResponse(Guid ConversationId, List<ChatMessagesDto> messages);

public record ChatMessagesDto(string? MessageId, string MessageText, string ChatRole, DateTimeOffset CreatedAt);