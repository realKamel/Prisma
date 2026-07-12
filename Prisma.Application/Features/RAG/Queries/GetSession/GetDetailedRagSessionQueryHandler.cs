using MediatR;
using Microsoft.Extensions.AI;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.RAG.Queries.GetSession;

public class
    GetDetailedRagSessionQueryHandler(IRagQuestionAnswering ragServices, IUnitOfWork uow)
    : IRequestHandler<GetDetailedRagSessionQuery,
        Result<GetDetailedRagSessionQueryResponse>>

{
    public async Task<Result<GetDetailedRagSessionQueryResponse>> Handle(GetDetailedRagSessionQuery request,
        CancellationToken cancellationToken)
    {
        var sessionRepo = uow.GetOrCreateRepository<ChatSession, Guid>();
        var session = await sessionRepo.GetByIdAsync(request.Id, cancellationToken);

        if (session is null)
        {
            return Result<GetDetailedRagSessionQueryResponse>.Failure("Session not found");
        }

        var chatMessages =
            await ragServices.GetChatMessagesAsync(session.SerializedSessionJson, cancellationToken);

        var messages = chatMessages
            .Where(m => m.Role != ChatRole.System || m.Role != ChatRole.Tool)
            .Select(m =>
                new ChatMessagesDto(m.MessageId, m.Text, m.Role.ToString(),
                    m.CreatedAt ?? DateTimeOffset.UtcNow))
            .ToList() ?? [];

        return new GetDetailedRagSessionQueryResponse(session.Id, messages);
    }
}