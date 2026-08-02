using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.RAG.Queries.GetAllSessions;

public record GetAllSessionsQuery() : IRequest<Result<List<GetRagSessionQuery>>>;

public record GetRagSessionQuery(Guid SessionId, string Title);