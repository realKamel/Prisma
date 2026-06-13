using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

namespace Prisma.API.Features.Lessons;


    public class LessonsController(IMediator _mediator) : ApiController
    {
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetLessonDetails(int id, CancellationToken cancellationToken)
        {
        Guid studentId = default;
        if(User.Identity != null && User.Identity.IsAuthenticated)
            studentId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            var query = new GetLessonDetailsQuery(id, studentId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(new { data = result.Data });
        }
    }

