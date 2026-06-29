using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Storage.Commands.DeleteFile;
using Prisma.Application.Features.Storage.Commands.GetDownloadUrl;
using Prisma.Application.Features.Storage.Commands.UploadFile;


namespace Prisma.API.Features.Student;

public class StorageController(IMediator mediator) : ApiController
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string bucketName, [FromQuery] string objectKey, CancellationToken cancellationToken)
    {
        using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadFileCommand(bucketName, objectKey, stream, file.ContentType), cancellationToken);
        return Ok(result);
    }

    [HttpGet("download")]
    public async Task<IActionResult> GetDownloadUrl([FromQuery] string bucketName, [FromQuery] string objectKey, [FromQuery] int expiryMinutes = 60)
    {
        var result = await mediator.Send(new GetDownloadUrlQuery(bucketName, objectKey, expiryMinutes));
        return Ok(result);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] string bucketName, [FromQuery] string objectKey, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteFileCommand(bucketName, objectKey), cancellationToken);
        return Ok(result);
    }
}