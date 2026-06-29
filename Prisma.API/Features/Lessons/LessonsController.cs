using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterialCommand;
using Prisma.Application.Features.Lessons.Commands.SubmitAssignmentCommand;
using Prisma.Application.Features.Lessons.Commands.ToggleLessonStatus;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonDetails;
using Prisma.Application.Features.Lessons.Commands.UploadLessonMaterials;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonExpired;
using Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;
using Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;
using Prisma.Application.Features.Lessons.Queries.GetLessonStatus;

namespace Prisma.API.Features.Lessons;

public class LessonsController(IMediator _mediator) : ApiController
{
    [HttpGet("details/{LessonId}")]
    public async Task<IActionResult> GetLessonDetails([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var query = new GetLessonDetailsQuery(int.Parse(LessonId));
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("watch/{LessonId}")]
    public async Task<IActionResult> GetLessonPlayerDetails([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var query = new GetLessonPlayerQuery(int.Parse(LessonId));
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("status/{LessonId}")]
    public async Task<IActionResult> GetLessonStatus([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var query = new GetLessonStatusQuery(int.Parse(LessonId));
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetPrepDataForAdd(CancellationToken cancellationToken)
    {
        var query = new GetLessonFormOptionsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("expired-details/{LessonId}")]
    public async Task<IActionResult> GetExpiredLessonDetails([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var query = new GetLessonExpiredQuery(int.Parse(LessonId));
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("editor/{LessonId}")]
    [ProducesResponseType<Result<LessonEditorResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLessonEditorDetails([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLessonEditorDetailsQuery(int.Parse(LessonId)), cancellationToken);
        return Ok(result);
    }

    [HttpPost("add")]
    [ProducesResponseType<Result<int>>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateLesson([FromBody] CreateLessonDetailsCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }


    [HttpDelete("{LessonId}")]
    [ProducesResponseType<Result<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteLesson([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteLessonCommand(int.Parse(LessonId)), cancellationToken);
        return Ok(result);
    }



    [HttpPut("editor/{LessonId}")]
    [ProducesResponseType<Result<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLessonEditorDetails(
        [FromRoute] string LessonId,
        [FromBody] UpdateLessonDetailsCommand command,
        CancellationToken cancellationToken)
    {
        var finalCommand = command with { Id = int.Parse(LessonId) };

        var result = await _mediator.Send(finalCommand, cancellationToken);

        return Ok(result);
    }
    [HttpPatch("toggle-status/{LessonId}")]
    [ProducesResponseType<Result<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleLessonStatus([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleLessonStatusCommand(int.Parse(LessonId)), cancellationToken);

        return Ok(result);
    }
    [HttpPost("upload-materials/{LessonId}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<Result<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadMaterials(
        [FromRoute] string LessonId,
        [FromForm] UploadMaterialsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UploadLessonMaterialsCommand(int.Parse(LessonId), request.Files);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("delete-material/{LessonId:int}/{MaterialId:int}")]
    [ProducesResponseType<Result<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMaterial(
        [FromRoute] int LessonId,
        [FromRoute] int MaterialId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteLessonMaterialCommand(LessonId, MaterialId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("materials/{LessonId}")]
    [ProducesResponseType<Result<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLessonMaterials([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var query = new GetLessonMaterialQuery(int.Parse(LessonId));
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{lessonId:int}/assignment/submit")]
    public async Task<IActionResult> SubmitAssignment(
    int lessonId,
    IFormFile file, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmitAssignmentCommand(lessonId, file),cancellationToken);
        return Ok(result);
    }
}

public class UploadMaterialsRequest
{
    public List<IFormFile> Files { get; set; } = new();
}