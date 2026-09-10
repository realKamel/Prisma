using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.Application.Features.LandingPage.Queries.ExportLandingPage;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.API.Features.LandingPage;

[EnableRateLimiting(RateLimitPolicies.Public)]
public class LandingPageController(IMediator mediator) : ApiController
{
    [HttpGet("export/{email}")]
    public async Task<Result<TeacherLandingSettings>> ExportLandingPage(
        string email,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(new ExportLandingPageQuery(email), cancellationToken);
    }
}
