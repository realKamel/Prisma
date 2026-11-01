using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.RAG.Queries.GetSession;

public sealed record GetDetailedRagSessionQuery(Guid Id)
    : IRequest<Result<GetDetailedRagSessionQueryResponse>>;

public sealed record GetDetailedRagSessionQueryResponse(
    Guid ConversationId,
    IList<ChatMessagesDto> messages
);

public sealed record ChatMessagesDto(
    string? MessageId,
    string MessageText,
    string ChatRole,
    DateTimeOffset CreatedAt
);
