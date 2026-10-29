using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Abstractions.Services.VideoServices;

public interface IVideoLibraryService
{
    Task<VideoLibrary> CreateLibrary(string libraryName, CancellationToken cancellationToken);
}
