using System.Linq.Expressions;
using Ardalis.Specification;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.RAG.Queries.GetAllSessions;

public class GetAllSessionsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUserService)
    : IRequestHandler<GetAllSessionsQuery, Result<List<GetRagSessionQuery>>>
{
    public class ChatSessionSpec<TResult> : Specification<ChatSession, TResult>
    {
        public ChatSessionSpec(Guid? userId, Expression<Func<ChatSession, TResult>> selector)
        {
            Query.Where(s => s.UserId == userId)
                .AsNoTracking()
                .Select(selector);
        }
    }

    public async Task<Result<List<GetRagSessionQuery>>> Handle(GetAllSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
        {
            // throw new UnauthorizedException();
            userId = Guid.Empty;
        }


        var repo = uow.GetOrCreateRepository<ChatSession, Guid>();

        var sessions = await repo
            .ListAsync(new ChatSessionSpec<GetRagSessionQuery>
                (userId, s =>
                    new GetRagSessionQuery(s.Id, s.Title)),
                cancellationToken);

        return sessions;
    }
}