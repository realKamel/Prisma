using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class Video : BaseEntity
{
    private Video() { }

    public int VideoLibraryId { get; private set; }
    public VideoLibrary VideoLibrary { get; private set; } = null!;

    public int SectionId { get; private set; }
    public Section Section { get; private set; } = null!;

    public Guid ProviderVideoId { get; private set; }
    public long ProviderVideoLibraryId { get; private set; }

    public string Title { get; private set; } = null!;

    public long StorageSizeBytes { get; private set; }

    public TimeDuration TimeDuration { get; private set; } = TimeDuration.Zero;

    public VideoProcessingStatus ProcessingStatus { get; private set; }

    public Uri? ThumbnailUrl { get; private set; }
}
