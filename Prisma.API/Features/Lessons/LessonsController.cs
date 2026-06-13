using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

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
    }

