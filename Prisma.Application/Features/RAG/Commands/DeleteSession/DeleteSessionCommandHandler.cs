using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
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
            throw new NotFoundException("ChatSession", request.Id);
        }

        repo.Delete(entity);

        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}