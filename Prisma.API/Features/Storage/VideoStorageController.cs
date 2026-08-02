using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result;
using Prisma.API.Common;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Storage.Commands.DeleteVideo;
using Prisma.Application.Features.Storage.Commands.MuxWebhook;
using Prisma.Application.Features.Storage.Queries.GetAudioUrl;
using Prisma.Application.Features.Storage.Queries.GetUploadUrl;
using Prisma.Application.Features.Storage.Queries.GetVideoUrl;


namespace Prisma.API.Features.Storage;

public class VideoStorageController(IMediator mediator) : ApiController
{
    [HttpGet("upload-url")]
    public async Task<Result<VideoUploadResult>> GetUploadUrl([FromQuery] int sectionId)
    {
        var result = await mediator.Send(new GetUploadUrlQuery(sectionId));
        return Result<VideoUploadResult>.Success(result);
    }

    [HttpGet("video-url")]
    public async Task<Result<string>> GetVideoUrl([FromQuery] string objectKey)
    {
        var url = await mediator.Send(new GetVideoUrlQuery(objectKey));
        return Result<string>.Success(url);
    }

    [HttpGet("audio-url")]
    public async Task<Result<string>> GetAudioUrl([FromQuery] string objectKey)
    {
        var url = await mediator.Send(new GetAudioUrlQuery(objectKey));
        return Result<string>.Success(url);
    }

    [HttpDelete("delete")]
    public async Task<Result> Delete([FromQuery] string objectKey, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteVideoCommand(objectKey), cancellationToken);
        return Result.Success();
    }

    [HttpPost("mux-webhook")]
    public async Task<Result> Handle([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        var eventType = payload.GetProperty("type").GetString();

        if (eventType != "video.asset.ready")
            return Result.Success();

        var data = payload.GetProperty("data");
        var assetId = data.GetProperty("id").GetString()!;
        var playbackId = data.GetProperty("playback_ids")[0].GetProperty("id").GetString()!;
        var passthrough = data.GetProperty("passthrough").GetString()!;

        if (!int.TryParse(passthrough, out var sectionId))
            return Result.Error("Invalid passthrough section id.");

        return await mediator.Send(new MuxWebhookCommand(assetId, playbackId, sectionId), cancellationToken);
    }
}