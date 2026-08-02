using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result;
using Prisma.API.Common;
using Prisma.Application.Features.Sections.Commands.CompleteSection;
using Prisma.Application.Features.Sections.Commands.CreateSectionProgress;
using Prisma.Application.Features.Sections.Commands.SaveSectionProgress;

namespace Prisma.API.Features.Section;

public class SectionProgressController(IMediator mediator) : ApiController
{
    [HttpPost("{sectionId}/progress/start")]
    public async Task<Result> Start(int sectionId, CancellationToken cancellationToken)
    {
        return await mediator.Send(new CreateSectionProgressCommand(sectionId), cancellationToken);
    }

    [HttpPut("{sectionId}/progress")]
    public async Task<Result> Save(int sectionId, [FromBody] SaveProgressRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new SaveSectionProgressCommand(sectionId, request.WatchedSeconds), cancellationToken);
    }

    [HttpPost("{sectionId}/progress/complete")]
    public async Task<Result> Complete(int sectionId, CancellationToken cancellationToken)
    {
        return await mediator.Send(new CompleteSectionCommand(sectionId), cancellationToken);
    }
}

public record SaveProgressRequest(double WatchedSeconds);