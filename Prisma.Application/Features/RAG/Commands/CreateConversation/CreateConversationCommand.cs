using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.RAG.Queries.GetSession;

namespace Prisma.Application.Features.RAG.Commands.CreateConversation;

public record CreateConversationCommand(string Question) : IRequest<Result<GetDetailedRagSessionQueryResponse>>;