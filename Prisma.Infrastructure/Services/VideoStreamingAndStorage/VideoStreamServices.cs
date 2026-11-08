using Ardalis.Result;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Abstractions.VideoServices;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.CreateVideo;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Stream.SharedDtos;

namespace Prisma.Infrastructure.Services.VideoStreamingAndStorage;

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

    private static VideoProcessingStatus MatchStatus(VideoModelStatus status)
    {
        return status switch
        {
            VideoModelStatus.Created => VideoProcessingStatus.Created,
            VideoModelStatus.Uploaded => VideoProcessingStatus.Uploaded,
            VideoModelStatus.Processing => VideoProcessingStatus.Processing,
            VideoModelStatus.Transcoding => VideoProcessingStatus.Transcoding,
            VideoModelStatus.Finished => VideoProcessingStatus.Finished,
            VideoModelStatus.Error => VideoProcessingStatus.Error,
            VideoModelStatus.UploadFailed => VideoProcessingStatus.UploadFailed,
            VideoModelStatus.JitSegmenting => VideoProcessingStatus.JitSegmenting,
            VideoModelStatus.JitPlaylistsCreated => VideoProcessingStatus.JitPlaylistsCreated,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Unknown video status."
            ),
        };
    }
}
