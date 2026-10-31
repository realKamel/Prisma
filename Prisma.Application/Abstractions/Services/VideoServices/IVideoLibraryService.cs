using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Abstractions.Services.VideoServices;

public interface IVideoLibraryService
{
    //post
    Task<Result<VideoLibraryCreateResult>> CreateLibrary(
        string name,
        CancellationToken cancellationToken,
        EncodingTier encodingTier,
        string enabledResolutions = "480p,720p,1080p"
    );

    //put
    Task<Result<VideoLibrary>> EnsureLibraryExistsAsync(
        string libraryName,
        CancellationToken cancellationToken
    );
}
