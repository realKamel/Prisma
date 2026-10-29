using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Errors;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class Video : BaseEntity
{
    private Video() { }

    public int VideoLibraryId { get; private set; }
    public VideoLibrary VideoLibrary { get; private set; } = null!;

    public int? SectionId { get; private set; }
    public Section? Section { get; private set; } = null!;

    public Guid ProviderVideoId { get; private set; }
    public long ProviderVideoLibraryId { get; private set; }

    public string Title { get; private set; } = null!;

    public long StorageSizeBytes { get; private set; }

    public TimeDuration TimeDuration { get; private set; } = TimeDuration.Zero;

    public VideoProcessingStatus ProcessingStatus { get; private set; }

    public Uri? ThumbnailUrl { get; private set; }

    /// <summary>
    /// Factory method returning a Result encapsulating the new Video entity or validation errors.
    /// </summary>
    public static Result<Video> Create(
        Guid providerVideoId,
        long providerVideoLibraryId,
        string title,
        long storageSizeBytes,
        long durationInSeconds,
        VideoProcessingStatus processingStatus,
        Uri? thumbnailUrl = null
    )
    {
        // if (videoLibraryId <= 0)
        // {
        //     return Result.Invalid(
        //         new ValidationError
        //         {
        //             Identifier = nameof(VideoLibraryId),
        //             ErrorMessage = "Video library ID must be greater than zero.",
        //             ErrorCode = DomainErrors.DomainValdtionErrors.Video.VideoLibraryIdRequired,
        //         }
        //     );
        // }

        if (providerVideoId == Guid.Empty)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(ProviderVideoId),
                    ErrorMessage = "Provider video ID cannot be empty.",
                    ErrorCode = DomainErrors.DomainValdtionErrors.Video.ProviderVideoIdRequired,
                }
            );
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(Title),
                    ErrorMessage = "Video title cannot be empty.",
                    ErrorCode = DomainErrors.DomainValdtionErrors.Video.TitleRequired,
                }
            );
        }

        if (storageSizeBytes < 0)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(StorageSizeBytes),
                    ErrorMessage = "Storage size cannot be negative.",
                    ErrorCode = DomainErrors.DomainValdtionErrors.Video.StorageSizeInvalid,
                }
            );
        }

        var video = new Video
        {
            // VideoLibraryId = videoLibraryId,
            ProviderVideoId = providerVideoId,
            ProviderVideoLibraryId = providerVideoLibraryId,
            Title = title.Trim(),
            StorageSizeBytes = storageSizeBytes,
            TimeDuration = TimeDuration.Create(durationInSeconds),
            ProcessingStatus = processingStatus,
            ThumbnailUrl = thumbnailUrl,
        };

        return Result.Success(video);
    }

    public void AttachToSection(int sectionId)
    {
        SectionId = sectionId;
    }

    public void AttachToLibrary(int videoLibraryId)
    {
        VideoLibraryId = videoLibraryId;
    }
}
