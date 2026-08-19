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

public class LessonsController(IMediator _mediator) : ApiController
{
    [HttpGet("{id:int}/details")]
    public async Task<Result<LessonDetailsDto>> GetLessonDetails(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetLessonDetailsQuery(id);
        return await _mediator.Send(query, cancellationToken);
    }

    [HttpGet("{id:int}/watch")]
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

    [HttpGet("{id:int}/expired-details")]
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
        return await _mediator.Send(new GetLessonEditorDetailsQuery(id), cancellationToken);
    }

    [HttpPost]
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
    public async Task<Result> DeleteLesson([FromRoute] int id, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new DeleteLessonCommand(id), cancellationToken);
    }

    [HttpPut("{id:int}/editor")]
    [Consumes("multipart/form-data")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result<UpdateLessonResponse>> UpdateLessonEditorDetails(
        [FromRoute] int id,
        [FromForm] UpdateLessonDetailsCommand command,
        CancellationToken cancellationToken
    )
    {
        var finalCommand = command with { Id = id };

        var result = await _mediator.Send(finalCommand, cancellationToken);

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
    public async Task<Result> ToggleLessonStatus(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var result = await _mediator.Send(new ToggleLessonStatusCommand(id), cancellationToken);

        return result;
    }

    [HttpPost("{id:int}/materials")]
    [Consumes("multipart/form-data")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result> UploadMaterials(
        [FromRoute] int id,
        [FromForm] UploadMaterialsRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new UploadLessonMaterialsCommand(id, request.Files);
        return await _mediator.Send(command, cancellationToken);
    }

    [HttpDelete("{id:int}/materials/{materialId:int}")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result> DeleteMaterial(
        [FromRoute] int id,
        [FromRoute] int MaterialId,
        CancellationToken cancellationToken
    )
    {
        var command = new DeleteLessonMaterialCommand(id, MaterialId);
        return await _mediator.Send(command, cancellationToken);
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
        var result = await _mediator.Send(query, cancellationToken);
        return result;
    }

    [HttpPost("{lessonId:int}/assignments")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result> SubmitAssignment(
        int lessonId,
        IFormFile file,
        CancellationToken cancellationToken
    )
    {
        return await _mediator.Send(new SubmitAssignmentCommand(lessonId, file), cancellationToken);
    }

    [HttpDelete("{lessonId:int}/assignments/submission")]
    [ExpectedFailures(
        ResultStatus.CriticalError,
        ResultStatus.Error,
        ResultStatus.Unauthorized,
        ResultStatus.Invalid,
        ResultStatus.Unauthorized
    )]
    public async Task<Result> DeleteSubmission(int lessonId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new DeleteSubmissionCommand(lessonId), cancellationToken);
    }
}

public class UploadMaterialsRequest
{
    public List<IFormFile> Files { get; set; } = new();
}
