using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.TeacherPreferences.Commands.UpdateAccentColor;


namespace Prisma.API.Features.Teacher;

[Authorize(Roles = AppRoles.Teacher)]
[Route("api/v1/teacher/preferences")]
public class TeacherPreferencesController(ISender sender) : ApiController
{
    [HttpPut("accent")]
    public async Task<ActionResult> UpdateAccentColor(
        [FromBody] UpdateAccentColorCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}