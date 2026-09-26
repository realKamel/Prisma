using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Enrollments.Commands.MarkEnrollmentCompleted;

namespace Prisma.API.Features.Enrollments;

[EnableRateLimiting(RateLimitPolicies.UserWrite)]
[Authorize]
public sealed class EnrollmentsController(ISender mediator) : ApiController
{
    [Authorize(Roles = AppRoles.Student)]
    [HttpPatch("{enrollmentId:guid}/completed")]
    public async Task<Result> MarkEnrollmentCompleted(Guid enrollmentId, CancellationToken cancellationToken)
    {
        return await mediator.Send(new MarkEnrollmentCompletedCommand(enrollmentId), cancellationToken);
    }
}