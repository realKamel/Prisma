using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Sections.Commands.CompleteSection;
using Prisma.Application.Features.Sections.Commands.CreateSectionProgress;
using Prisma.Application.Features.Sections.Commands.SaveSectionProgress;

namespace Prisma.API.Features.Section;

public class SectionProgressController(IMediator mediator) : ApiController
{
    [HttpPost("{sectionId}/progress/start")]
    public async Task<ActionResult> Start(int sectionId, CancellationToken cancellationToken)
    {
        await mediator.Send(new CreateSectionProgressCommand(sectionId), cancellationToken);
        return Ok();
    }

    [HttpPut("{sectionId}/progress")]
    public async Task<ActionResult> Save(int sectionId, [FromBody] SaveProgressRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new SaveSectionProgressCommand(sectionId, request.WatchedSeconds), cancellationToken);
        return Ok();
    }

    [HttpPost("{sectionId}/progress/complete")]
    public async Task<ActionResult> Complete(int sectionId, CancellationToken cancellationToken)
    {
        await mediator.Send(new CompleteSectionCommand(sectionId), cancellationToken);
        return Ok();
    }
}

public record SaveProgressRequest(double WatchedSeconds);