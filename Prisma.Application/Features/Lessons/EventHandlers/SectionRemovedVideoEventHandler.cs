using Ardalis.Result;
using Prisma.Application.Abstractions.Services.VideoServices;
using Prisma.Application.Common.Interfaces;
using Prisma.Domain.DomainEvents.LessonEvents;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.VideoSpecs;

namespace Prisma.Application.Features.Lessons.EventHandlers;

internal static class SectionRemovedVideoEventHandler
{
    public static async Task Handle(
        SectionRemovedVideoEvent message,
        IUnitOfWork unitOfWork,
        ISecretCipher secretCipher,
        IVideoStreamServices videoStreamServices,
        CancellationToken cancellationToken
    )
    {
        var videoRepository = unitOfWork.GetOrCreateRepository<Video, int>();

        var spec = new VideoByIdWithVideoLibrary(message.VideoId, asNoTracking: false);

        var video = await videoRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (video is null || string.IsNullOrWhiteSpace(video.VideoLibrary.LibraryApiKey))
        {
            return;
        }

        var decryptedApiKey = secretCipher.Decrypt(video.VideoLibrary.LibraryApiKey);

        var result = await videoStreamServices.DeleteVideoAsync(
            libraryId: video.ProviderVideoLibraryId,
            videoId: video.ProviderVideoId,
            libraryApiKey: decryptedApiKey,
            cancellationToken
        );

        if (result.IsNotFound())
        {
            // Remote video is already gone; continue local cleanup.
        }
        else if (result.IsUnauthorized() || result.IsForbidden())
        {
            throw new InvalidOperationException(
                $"Provider rejected deletion of video {video.Id}. "
                    + "Check the library credentials and permissions."
            );
        }
        else if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to delete provider video {video.Id}.");
        }

        videoRepository.Delete(video);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
