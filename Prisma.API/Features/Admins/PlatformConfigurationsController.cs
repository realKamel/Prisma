using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.TeacherPreferences.Commands.UpdateAccentColor;

namespace Prisma.API.Features.Admins;

[Authorize(Roles = AppRoles.Admin)]
public class PlatformConfigurationsController(ISender sender) : ApiController
{
    [HttpPut("accent")]
    public async Task<Result> UpdateAccentColor(
        [FromBody] UpdateAccentColorCommand command,
        CancellationToken ct
    )
    {
        var result = await sender.Send(command, ct);
        return result;
    }
}
