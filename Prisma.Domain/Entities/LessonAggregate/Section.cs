using Prisma.Domain.Common;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class Section : BaseEntity
{
    public Guid? PublicId { get; set; } = Guid.CreateVersion7();
    public string Title { get; set; } = string.Empty;
    public string? ContentURL { get; set; }

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public string? Transcript { get; set; }
    public string? TranscriptSummary { get; set; }
    public int SortOrder { get; set; }

    public ICollection<SectionProgress> Progresses { get; set; } = [];

    public TimeSpan? Duration => TimeDuration?.Value;
    public TimeDuration? TimeDuration { get; set; }
    public bool IsPreview { get; set; } = false;

    //video streaming
    public string? UploadId { get; set; }
    public string? AssetId { get; set; }
    public string? PlaybackId { get; set; }
}
