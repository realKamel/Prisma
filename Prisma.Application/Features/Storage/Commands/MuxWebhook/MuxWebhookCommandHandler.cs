using MediatR;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Storage.Commands.MuxWebhook;

public class MuxWebhookCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<MuxWebhookCommand, Result>
{
    public async Task<Result> Handle(MuxWebhookCommand request, CancellationToken cancellationToken)
    {
        var sectionRepo = unitOfWork.GetOrCreateRepository<Section, int>();
        var section = await sectionRepo.GetByIdAsync(request.SectionId, cancellationToken);

        if (section is null) return Result.NotFound($"Section with id '{request.SectionId}' was not found");

        section.AssetId = request.AssetId;
        section.PlaybackId = request.PlaybackId;
        sectionRepo.Update(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}