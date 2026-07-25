using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.TeacherPreferences.Queries.GetAccentColor;

namespace Prisma.API.Features;

[AllowAnonymous]
[Route("api/v1/preferences")]
public class PreferencesController(ISender sender) : ApiController
{
    [HttpGet("accent")]
    public async Task<ActionResult> GetAccentColor([FromQuery] string teacherEmail, CancellationToken ct)
    {
        var result = await sender.Send(new GetAccentColorQuery(teacherEmail), ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}