using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Storage.Commands.MuxWebhook;

public class MuxWebhookCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<MuxWebhookCommand>
{
    public async Task Handle(MuxWebhookCommand request, CancellationToken cancellationToken)
    {
        var sectionRepo = unitOfWork.GetOrCreateRepository<Section, int>();
        var section = await sectionRepo.GetByIdAsync(request.SectionId, cancellationToken);

        if (section is null) throw new NotFoundException(nameof(Section), request.SectionId);

        section.AssetId = request.AssetId;
        section.PlaybackId = request.PlaybackId;
        sectionRepo.Update(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}