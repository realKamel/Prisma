using MediatR;
using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.RAG.Commands.DeleteSession;

public class DeleteSessionCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteSessionCommand, Result>
{
    public async Task<Result> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var repo = uow.GetOrCreateRepository<ChatSession, Guid>();

        var entity = await repo.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.NotFound($"ChatSession with id '{request.Id}' was not found");
        }

        repo.Delete(entity);

        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}