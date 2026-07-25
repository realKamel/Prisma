using Prisma.API.Common;
using Prisma.Application.Features.LandingPage.Queries.ExportLandingPage;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Prisma.API.Features.LandingPage;

public class LandingPageController(IMediator mediator) : ApiController
{
    [HttpGet("export/{email}")]
    public async Task<ActionResult> ExportLandingPage(string email, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ExportLandingPageQuery(email), cancellationToken);

        return Ok(result);
    }
}