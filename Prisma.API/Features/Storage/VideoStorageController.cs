using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Storage.Commands.DeleteVideo;
using Prisma.Application.Features.Storage.Commands.MuxWebhook;
using Prisma.Application.Features.Storage.Queries.GetUploadUrl;
using Prisma.Application.Features.Storage.Queries.GetVideoUrl;


namespace Prisma.API.Features.Storage;

public class VideoStorageController(IMediator mediator) : ApiController
{
    [HttpGet("upload-url")]
    public async Task<IActionResult> GetUploadUrl([FromQuery] int sectionId)
    {
        var result = await mediator.Send(new GetUploadUrlQuery(sectionId));
        return Ok(result);
    }
    [HttpGet("video-url")]
    public async Task<IActionResult> GetVideoUrl([FromQuery] string objectKey)
    {
        var url = await mediator.Send(new GetVideoUrlQuery(objectKey));
        return Ok(url);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] string objectKey, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteVideoCommand(objectKey), cancellationToken);
        return Ok();
    }
    [HttpPost("mux-webhook")]
    public async Task<IActionResult> Handle([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        var eventType = payload.GetProperty("type").GetString();

        if (eventType != "video.asset.ready")
            return Ok();

        var data = payload.GetProperty("data");
        var assetId = data.GetProperty("id").GetString()!;
        var playbackId = data.GetProperty("playback_ids")[0].GetProperty("id").GetString()!;
        var passthrough = data.GetProperty("passthrough").GetString()!;

        if (!int.TryParse(passthrough, out var sectionId))
            return BadRequest();

        await mediator.Send(new MuxWebhookCommand(assetId, playbackId, sectionId), cancellationToken);

        return Ok();
    }
}