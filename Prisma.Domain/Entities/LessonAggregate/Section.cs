using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.DomainEvents.LessonEvents;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class Section : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.CreateVersion7();

    public string Title { get; set; } = string.Empty;
    public string? ContentURL { get; set; }

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public string? Transcript { get; set; }
    public string? TranscriptSummary { get; set; }
    public int SortOrder { get; set; }

    public ICollection<SectionProgress> Progresses { get; set; } = [];

    public TimeSpan? Duration => TimeDuration?.Value;
    public TimeDuration? TimeDuration { get; set; }

    public bool IsPreview { get; set; }

    // Video streaming
    public string? UploadId { get; set; }
    public string? AssetId { get; set; }
    public string? PlaybackId { get; set; }

    public Video? Video { get; set; }
    public int? VideoId { get; set; }

    //TODO: this must be private
    public Section() { } // EF Core

    public static Result<Section> Create(
        string title,
        int lessonId,
        int sortOrder,
        bool isPreviewable,
        long videoDurationInSeconds
    )
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<Section>.Invalid(
                new ValidationError(nameof(title), "Title is required.")
            );
        }

        if (lessonId <= 0)
        {
            return Result<Section>.Invalid(
                new ValidationError(nameof(lessonId), "Lesson ID must be positive.")
            );
        }

        if (sortOrder < 0)
        {
            return Result<Section>.Invalid(
                new ValidationError(nameof(sortOrder), "Sort order cannot be negative.")
            );
        }

        var timeDurationResult = TimeDuration.Create(videoDurationInSeconds);

        if (!timeDurationResult.IsSuccess)
        {
            return Result<Section>.Invalid(timeDurationResult.ValidationErrors);
        }

        var section = new Section
        {
            Title = title.Trim(),
            LessonId = lessonId,
            SortOrder = sortOrder,
            IsPreview = isPreviewable,
        };

        return Result<Section>.Success(section);
    }

    public void AttachVideo(int videoId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(videoId);
        VideoId = videoId;
    }

    public void AttachTranscript(string transcript)
    {
        Transcript = transcript;
    }

    public void AttachPlayback(string assetId, string playbackId)
    {
        AssetId = assetId;
        PlaybackId = playbackId;
    }

    public void RemoveVideo()
    {
        if (!VideoId.HasValue)
        {
            return;
        }

        var videoId = VideoId.Value;

        AddDomainEvent(
            new SectionRemovedVideoEvent(LessonId: LessonId, SectionId: Id, VideoId: videoId)
        );

        Video = null;
        VideoId = null;
    }
}
