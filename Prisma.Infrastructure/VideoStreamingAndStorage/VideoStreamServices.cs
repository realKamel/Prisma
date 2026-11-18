using Ardalis.Result;
using Prisma.Application.Abstractions.VideoServices;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.CreateVideo;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.SharedDtos;

namespace Prisma.Infrastructure.VideoStreamingAndStorage;

public class VideoStreamServices(IBunnyStreamVideosApi streamVideosApi) : IVideoStreamServices
{
    public async Task<Result<Video>> CreateVideoAsync(
        long libraryId,
        string libraryApiKey,
        string videoTitle,
        CancellationToken cancellationToken
    )
    {
        var model = new CreateVideoRequest { Title = videoTitle };
        var result = await streamVideosApi.CreateVideo(
            libraryId,
            libraryApiKey,
            model,
            cancellationToken
        );
        var videoResult = Video.Create(
            Guid.Parse(result.Guid),
            result.VideoLibraryId,
            result.Title,
            result.StorageSize,
            result.Length,
            MatchStatus(result.Status)
        );

        return videoResult;
    }

    public async Task<Result> DeleteVideoAsync(
        long libraryId,
        Guid videoId,
        string libraryApiKey,
        CancellationToken cancellationToken = default
    )
    {
        var response = await streamVideosApi.DeleteVideo(
            libraryId: libraryId,
            videoId: videoId.ToString(),
            libraryApiKey: libraryApiKey,
            cancellationToken: cancellationToken
        );
        return response.ToResult();
    }

    private static VideoStatus MatchStatus(VideoModelStatus status)
    {
        return status switch
        {
            VideoModelStatus.Created => VideoStatus.Created,
            VideoModelStatus.Uploaded => VideoStatus.Uploaded,
            VideoModelStatus.Processing => VideoStatus.Processing,
            VideoModelStatus.Transcoding => VideoStatus.Transcoding,
            VideoModelStatus.Finished => VideoStatus.Finished,
            VideoModelStatus.Error => VideoStatus.Error,
            VideoModelStatus.UploadFailed => VideoStatus.UploadFailed,
            VideoModelStatus.JitSegmenting => VideoStatus.JitSegmenting,
            VideoModelStatus.JitPlaylistsCreated => VideoStatus.JitPlaylistsCreated,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown video status."
            ),
        };
    }
}
