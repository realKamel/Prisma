using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.Application.Features.Users.Commands.MarkTourSeen;
using Prisma.Application.Features.Users.Queries.GetMyTours;

namespace Prisma.API.Features.Tours;

[Authorize]
public sealed class ToursController(ISender mediator) : ApiController
{
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    [HttpPut("{tourKey}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized,
        ResultStatus.NotFound,
        ResultStatus.Conflict
    )]
    public async Task<Result> MarkSeen(string tourKey, CancellationToken ct)
    {
        return await mediator.Send(new MarkTourSeenCommand(tourKey), ct);
    }

    [HttpGet]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.NotFound
    )]
    public async Task<Result<IReadOnlyCollection<string>>> GetMyTours(
        CancellationToken ct)
    {
        return await mediator.Send(new GetMyToursQuery(), ct);
    }
}