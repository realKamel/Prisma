using Prisma.API.Common;
using Prisma.Application.Features.LandingPage.Queries.ExportLandingPage;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.API.Features.LandingPage;

public class LandingPageController(IMediator mediator) : ApiController
{
    [HttpGet("export/{email}")]
    public async Task<Result<TeacherLandingSettings>> ExportLandingPage(string email,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new ExportLandingPageQuery(email), cancellationToken);
    }
}