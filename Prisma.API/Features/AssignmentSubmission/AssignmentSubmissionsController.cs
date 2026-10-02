using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.Application.Features.AssignmentSubmissions.Queries.GetAssignmentSubmission;
using Prisma.Application.Features.AssignmentSubmissions.Commands.DeleteAssignmentSubmission;
using Prisma.Application.Features.AssignmentSubmissions.Commands.SubmitAssignment;

namespace Prisma.API.Features.AssignmentSubmission;

[Route("api/v{version:apiVersion}/lessons/{lessonId:int}/assignment-submission")]
[EnableRateLimiting(RateLimitPolicies.UserWrite)]
public class AssignmentSubmissionsController(IMediator mediator) : ApiController
{
    [HttpGet]
    [ExpectedFailures(ResultStatus.Unauthorized, ResultStatus.NotFound)]
    public async Task<Result<AssignmentSubmissionDto?>> GetSubmission(
        int lessonId,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(new GetAssignmentSubmissionQuery(lessonId), cancellationToken);
    }

    [HttpPost]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid
    )]
    public async Task<Result> SubmitAssignment(
        int lessonId,
        IFormFile file,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(new SubmitAssignmentCommand(lessonId, file), cancellationToken);
    }

    [HttpDelete]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.NotFound
    )]
    public async Task<Result> DeleteSubmission(int lessonId, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteAssignmentSubmissionCommand(lessonId), cancellationToken);
    }
}