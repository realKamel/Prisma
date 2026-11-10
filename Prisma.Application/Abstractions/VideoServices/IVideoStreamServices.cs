using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Abstractions.VideoServices;

public interface IVideoStreamServices
{
    Task<Result<Video>> CreateVideoAsync(
        long libraryId,
        string libraryApiKey,
        string videoTitle,
        CancellationToken cancellationToken
    );

    Task<Result> DeleteVideoAsync(
        long libraryId,
        Guid videoId,
        string libraryApiKey,
        CancellationToken cancellationToken = default
    );
}
