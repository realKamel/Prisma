using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.LandingPage.Queries.ExportLandingPage;
namespace Prisma.API.Features.LandingPage;
using MediatR;
using Microsoft.AspNetCore.Mvc;


    public class LandingPageController : ApiController
{
        private readonly IMediator _mediator;

        public LandingPageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("export/{email}")]
        public async Task<IActionResult> ExportLandingPage( string email , CancellationToken cancellationToken )
        {
            var query = new ExportLandingPageQuery(email);

            var result = await _mediator.Send(query, cancellationToken);
            if (!result.Succeeded)
                return BadRequest(result);
            return Ok(result);
        }
    }
