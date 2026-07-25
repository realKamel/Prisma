using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Storage.Commands.DeleteFile;
using Prisma.Application.Features.Storage.Commands.UploadFile;
using Prisma.Application.Features.Storage.Queries.GetDownloadUrl;

namespace Prisma.API.Features.Storage;

public class StorageController(IMediator mediator) : ApiController
{
    [HttpPost("upload")]
    public async Task<ActionResult> Upload(IFormFile file, [FromQuery] string bucketName, [FromQuery] string objectKey,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadFileCommand(bucketName, objectKey, stream, file.ContentType),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("download")]
    public async Task<ActionResult> GetDownloadUrl([FromQuery] string bucketName, [FromQuery] string objectKey,
        [FromQuery] int expiryMinutes = 60)
    {
        var result = await mediator.Send(new GetDownloadUrlQuery(bucketName, objectKey, expiryMinutes));
        return Ok(result);
    }

    [HttpDelete("delete")]
    public async Task<ActionResult> Delete([FromQuery] string bucketName, [FromQuery] string objectKey,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteFileCommand(bucketName, objectKey), cancellationToken);
        return Ok(result);
    }
}