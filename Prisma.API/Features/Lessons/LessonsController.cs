using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;
using Prisma.Application.Features.Lessons.Commands.UpdateLessonDetails; // 🌟 الـ Namespace الخاص بـ الـ Editor الجديد
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonExpired;
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

    [HttpGet("expired-details/{LessonId}")]
    public async Task<IActionResult> GetExpiredLessonDetails([FromRoute] string LessonId, CancellationToken cancellationToken)
    {
        var query = new GetLessonExpiredQuery(int.Parse(LessonId));
        var result = await _mediator.Send(query, cancellationToken);
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
}