using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prisma.API.Common;
using Prisma.API.Common.RateLimitConfigurations;
using Prisma.API.Features.Lessons.RequestsAndResponses;
using Prisma.API.Filters;
using Prisma.Application.Features.Lessons.Commands.AddLesson;
using Prisma.Application.Features.Lessons.Commands.CreateLessonDetailsCommand;
using Prisma.Application.Features.Lessons.Commands.CreateLessonSection;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;
using Prisma.Application.Features.Lessons.Commands.RenewSectionUploadLink;
using Prisma.Application.Features.Lessons.Commands.ToggleLessonStatusCommand;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonCommand;
using Prisma.Application.Features.Lessons.Commands.UploadLessonMaterialsCommand;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonExpired;
using Prisma.Application.Features.Lessons.Queries.GetLessonFormOptions;
using Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;
using Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;
using Prisma.Application.Features.Lessons.Queries.GetLessonStatus;

namespace Prisma.API.Features.Lessons;

public sealed class LessonsController(IMediator mediator) : ApiController
{
    [HttpGet("{id:Guid}/details")]
    public async Task<Result<LessonDetailsDto>> GetLessonDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonDetailsQuery(id);
        return await mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:Guid}/watch")]
    public async Task<Result<LessonPlayerResult>> GetLessonPlayerDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonPlayerQuery(id);
        return await mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:Guid}/status")]
    public async Task<Result<LessonStatusResponse>> GetLessonStatus(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(new GetLessonStatusQuery(id), cancellationToken);
    }

    [HttpGet("options")]
    public async Task<Result<LessonFormOptionsResponseDto>> GetPrepDataForAdd(
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonFormOptionsQuery();
        return await mediator.Send(query, cancellationToken);
    }

    // [HttpGet("options/v2")]
    // public async Task<Result<LessonFormOptionsResponseDto>> GetPrepDataForAddV2(
    //     CancellationToken cancellationToken
    // )
    // {
    //     var query = new GetLessonFormOptionsQuery();
    //     return await mediator.Send(query, cancellationToken);
    // }

    [HttpGet("{id:Guid}/expired-details")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<LessonExpiredDto>> GetExpiredLessonDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonExpiredQuery(id);
        return await mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:int}/editor")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<LessonEditorResponseDto>> GetLessonEditorDetails(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(new GetLessonEditorDetailsQuery(id), cancellationToken);
    }

    [HttpPost]
    [Idempotent]
    [Consumes("multipart/form-data")]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result<CreateLessonResponse>> CreateLesson(
        [FromForm] CreateLessonDetailsCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpPost("v2")]
    [Idempotent]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result<AddLessonResponse>> AddLesson(
        [FromBody] AddLessonCommand command,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(command, cancellationToken);
    }

    [HttpPost("{id:guid}/sections")]
    [Idempotent]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<
        Result<CreateSectionAndAttachVideoUploadLinkResponse>
    > CreateSectionAndAttachVideo(
        [FromRoute] Guid id,
        [FromBody] CreateSectionAndAttachVideoUploadLinkRequest request,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new CreateLessonSectionCommand(
                id,
                request.Title,
                request.VideoDurationInSeconds,
                request.IsPreviewable
            ),
            cancellationToken
        );
    }

    [HttpPatch("{id:guid}/sections/{sectionId:guid}")]
    [Idempotent]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result<RenewSectionUploadLinkResponse>> RenewSectionVideoUploadLink(
        [FromRoute] Guid id,
        [FromRoute] Guid sectionId,
        [FromBody] RenewSectionUploadLinkRequest request,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new RenewSectionUploadLinkCommand(
                LessonId: id,
                SectionId: sectionId,
                VideoId: request.VideoId
            ),
            cancellationToken
        );
    }

    [HttpDelete("{id:guid}/sections/{sectionId:guid}")]
    [Idempotent]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result<RenewSectionUploadLinkResponse>> RemoveSection(
        [FromRoute] Guid id,
        [FromRoute] Guid sectionId,
        [FromBody] RenewSectionUploadLinkRequest request,
        CancellationToken cancellationToken
    )
    {
        return await mediator.Send(
            new RenewSectionUploadLinkCommand(
                LessonId: id,
                SectionId: sectionId,
                VideoId: request.VideoId
            ),
            cancellationToken
        );
    }

    [HttpDelete("{id:int}")]
    [Idempotent]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result> DeleteLesson([FromRoute] int id, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteLessonCommand(id), cancellationToken);
    }

    [HttpPut("{id:int}/editor")]
    [Idempotent]
    [Consumes("multipart/form-data")]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result<UpdateLessonResponse>> UpdateLessonEditorDetails(
        [FromRoute] int id,
        [FromForm] UpdateLessonDetailsCommand command,
        CancellationToken cancellationToken
    )
    {
        var finalCommand = command with { Id = id };

        var result = await mediator.Send(finalCommand, cancellationToken);

        return result;
    }

    [HttpPatch("{id:int}/toggle-status")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result> ToggleLessonStatus(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(new ToggleLessonStatusCommand(id), cancellationToken);

        return result;
    }

    [HttpPost("{id:int}/materials")]
    [Consumes("multipart/form-data")]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result> UploadMaterials(
        [FromRoute] int id,
        [FromForm] UploadMaterialsRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new UploadLessonMaterialsCommand(id, request.Files);
        return await mediator.Send(command, cancellationToken);
    }

    [HttpDelete("{id:int}/materials/{materialId:int}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    [EnableRateLimiting(RateLimitPolicies.UserWrite)]
    public async Task<Result> DeleteMaterial(
        [FromRoute] int id,
        [FromRoute] int materialId,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteLessonMaterialCommand(id, materialId);
        return await mediator.Send(command, cancellationToken);
    }

    [HttpGet("{id:int}/materials")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<List<LessonMaterialDto>>> GetLessonMaterials(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonMaterialQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return result;
    }
}

public sealed record UploadMaterialsRequest
{
    public IList<IFormFile> Files { get; init; } = [];
}
