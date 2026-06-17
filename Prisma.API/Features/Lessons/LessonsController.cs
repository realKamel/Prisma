using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
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

            return Ok( result);
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
}

