using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Lessons.Commands.GetLessonDetails;

namespace Prisma.API.Features.Lessons;


    public class LessonsController(IMediator _mediator) : ApiController
    {
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetLessonDetails(int id, CancellationToken cancellationToken)
        {
            Guid? studentId = User.Identity != null && User.Identity.IsAuthenticated
                ? Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!)
                : null;

            var command = new GetLessonDetailsCommand(id, studentId);
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(new { data = result.Data });
        }
    }

