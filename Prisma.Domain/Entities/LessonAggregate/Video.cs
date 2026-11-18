using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.DomainEvents.Videos;
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

    public VideoStatus Status { get; private set; } = VideoStatus.Created;

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
        VideoStatus processingStatus,
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
            ProviderVideoId = providerVideoId,
            ProviderVideoLibraryId = providerVideoLibraryId,
            Title = title.Trim(),
            StorageSizeBytes = storageSizeBytes,
            TimeDuration = TimeDuration.Create(durationInSeconds),
            Status = processingStatus,
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

    public Result MarkProcessingStarted(long? storageSizeBytes = null, Uri? thumbnailUrl = null)
    {
        return TransitionTo(
            VideoStatus.Processing,
            storageSizeBytes: storageSizeBytes,
            thumbnailUrl: thumbnailUrl
        );
    }

    public Result MarkTranscoding(long? storageSizeBytes = null, Uri? thumbnailUrl = null)
    {
        return TransitionTo(
            VideoStatus.Transcoding,
            storageSizeBytes: storageSizeBytes,
            thumbnailUrl: thumbnailUrl
        );
    }

    public Result MarkFinished(
        long? durationInSeconds = null,
        long? storageSizeBytes = null,
        Uri? thumbnailUrl = null
    )
    {
        return TransitionTo(
            VideoStatus.Finished,
            durationInSeconds: durationInSeconds,
            storageSizeBytes: storageSizeBytes,
            thumbnailUrl: thumbnailUrl
        );
    }

    public Result MarkJitSegmenting(long? storageSizeBytes = null, Uri? thumbnailUrl = null)
    {
        return TransitionTo(
            VideoStatus.JitSegmenting,
            storageSizeBytes: storageSizeBytes,
            thumbnailUrl: thumbnailUrl
        );
    }

    public Result MarkJitPlaylistsCreated(
        long? durationInSeconds = null,
        long? storageSizeBytes = null,
        Uri? thumbnailUrl = null
    )
    {
        return TransitionTo(
            VideoStatus.JitPlaylistsCreated,
            durationInSeconds: durationInSeconds,
            storageSizeBytes: storageSizeBytes,
            thumbnailUrl: thumbnailUrl
        );
    }

    public Result MarkFailed(
        bool isUploadFailure,
        string? reason = null,
        long? storageSizeBytes = null,
        Uri? thumbnailUrl = null
    )
    {
        var failureStatus = isUploadFailure ? VideoStatus.UploadFailed : VideoStatus.Error;

        return TransitionTo(
            failureStatus,
            storageSizeBytes: storageSizeBytes,
            thumbnailUrl: thumbnailUrl,
            failureReason: reason
        );
    }

    private Result TransitionTo(
        VideoStatus nextStatus,
        long? durationInSeconds = null,
        long? storageSizeBytes = null,
        Uri? thumbnailUrl = null,
        string? failureReason = null
    )
    {
        ArgumentNullException.ThrowIfNull(nextStatus);

        // Idempotent duplicate webhook for current status
        if (Status == nextStatus)
        {
            ApplyMetadata(durationInSeconds, storageSizeBytes, thumbnailUrl);
            return Result.Success();
        }

        if (!Status.CanTransitionTo(nextStatus))
        {
            // Ignore stale/out-of-order states after reaching a terminal state
            if (Status.IsTerminal)
            {
                ApplyMetadata(durationInSeconds, storageSizeBytes, thumbnailUrl);
                return Result.Success();
            }

            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(Status),
                    ErrorMessage =
                        $"Cannot transition video from {Status.Name} to {nextStatus.Name}.",
                }
            );
        }

        var wasReadyForPlayback = Status.IsReadyForPlayback;
        Status = nextStatus;
        ApplyMetadata(durationInSeconds, storageSizeBytes, thumbnailUrl);

        if (nextStatus.IsFailure)
        {
            AddDomainEvent(
                new VideoProcessingFailedDomainEvent(
                    Guid.CreateVersion7(),
                    Id,
                    ProviderVideoId,
                    ProviderVideoLibraryId,
                    nextStatus,
                    failureReason
                )
            );
        }
        else if (nextStatus.IsReadyForPlayback && !wasReadyForPlayback)
        {
            AddDomainEvent(
                new VideoProcessingCompletedDomainEvent(
                    Guid.CreateVersion7(),
                    Id,
                    ProviderVideoId,
                    ProviderVideoLibraryId,
                    SectionId,
                    TimeDuration.Seconds,
                    StorageSizeBytes,
                    ThumbnailUrl
                )
            );
        }

        return Result.Success();
    }

    private void ApplyMetadata(long? durationInSeconds, long? storageSizeBytes, Uri? thumbnailUrl)
    {
        if (durationInSeconds is > 0)
        {
            TimeDuration = TimeDuration.Create(durationInSeconds.Value);
        }

        if (storageSizeBytes is >= 0)
        {
            StorageSizeBytes = storageSizeBytes.Value;
        }

        if (thumbnailUrl is not null)
        {
            ThumbnailUrl = thumbnailUrl;
        }
    }
}
