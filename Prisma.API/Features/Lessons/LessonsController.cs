using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;
using Prisma.Application.Features.Lessons.Commands.DeleteAssignmentSubmissionCommand;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;
using Prisma.Application.Features.Lessons.Commands.SubmitAssignmentCommand;
using Prisma.Application.Features.Lessons.Commands.ToggleLessonStatus;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonCommand;
using Prisma.Application.Features.Lessons.Commands.UploadLessonMaterials;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonExpired;
using Prisma.Application.Features.Lessons.Queries.GetLessonFormOptions;
using Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;
using Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;
using Prisma.Application.Features.Lessons.Queries.GetLessonStatus;

namespace Prisma.API.Features.Lessons;

public class LessonsController(IMediator _mediator) : ApiController
{
    [HttpGet("details/{id:int}")]
    public async Task<Result<LessonDetailsDto>> GetLessonDetails(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonDetailsQuery(id);
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("watch/{id:int}")]
    public async Task<Result<LessonPlayerResult>> GetLessonPlayerDetails(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonPlayerQuery(id);
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:int}/status")]
    public async Task<Result<LessonStatusResponse>> GetLessonStatus(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonStatusQuery(id);
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("options")]
    public async Task<Result<LessonFormOptionsResponseDto>> GetPrepDataForAdd(
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonFormOptionsQuery();
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("expired-details/{id:int}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<LessonExpiredDto>> GetExpiredLessonDetails(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonExpiredQuery(id);
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("editor/{id:int}")]
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
        return await _mediator.Send(new GetLessonEditorDetailsQuery(id), cancellationToken);
    }

    [HttpPost("add")]
    [Consumes("multipart/form-data")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<CreateLessonResponse>> CreateLesson(
        [FromForm] CreateLessonDetailsCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpDelete("{id:int}")]
    public async Task<Result<string>> DeleteLesson(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        return await _mediator.Send(new DeleteLessonCommand(id), cancellationToken);
    }

    [HttpPut("editor/{LessonId:int}")]
    [Consumes("multipart/form-data")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<UpdateLessonResponse>> UpdateLessonEditorDetails(
        [FromRoute] int LessonId,
        [FromForm] UpdateLessonDetailsCommand command,
        CancellationToken cancellationToken
    )
    {
        var finalCommand = command with { Id = LessonId };

        var result = await _mediator.Send(finalCommand, cancellationToken);

        return result;
    }

    [HttpPatch("toggle-status/{id:int}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<string>> ToggleLessonStatus(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(new ToggleLessonStatusCommand(id), cancellationToken);

        return result;
    }

    [HttpPost("upload-materials/{id:int}")]
    [Consumes("multipart/form-data")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<string>> UploadMaterials(
        [FromRoute] int id,
        [FromForm] UploadMaterialsRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new UploadLessonMaterialsCommand(id, request.Files);

        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpDelete("delete-material/{LessonId:int}/{MaterialId:int}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<string>> DeleteMaterial(
        [FromRoute] int LessonId,
        [FromRoute] int MaterialId,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteLessonMaterialCommand(LessonId, MaterialId);
        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpGet("materials/{id:int}")]
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
        var result = await _mediator.Send(query, cancellationToken);
        return result;
    }

    [HttpPost("{lessonId:int}/assignment/submit")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<string>> SubmitAssignment(
        int lessonId,
        IFormFile file,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(
            new SubmitAssignmentCommand(lessonId, file),
            cancellationToken
        );
        return result;
    }

    [HttpDelete("{lessonId:int}/assignment/submission")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<string>> DeleteSubmission(
        int lessonId,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(new DeleteSubmissionCommand(lessonId), cancellationToken);
        return result;
    }
}

public class UploadMaterialsRequest
{
    public List<IFormFile> Files { get; set; } = new();
}
