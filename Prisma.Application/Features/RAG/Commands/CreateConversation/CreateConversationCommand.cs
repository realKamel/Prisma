using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.RAG.Queries.GetSession;

namespace Prisma.Application.Features.RAG.Commands.CreateConversation;

public record CreateConversationCommand(string Question) : IRequest<Result<GetDetailedRagSessionQueryResponse>>;