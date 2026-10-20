using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class Video : BaseEntity
{
    public long ProviderVideoId { get; set; }
    public string Title { get; set; }
    public long StorageSize { get; set; }
    public Uri? ThumbnailUrl { get; init; }

    // library relation (Library <-1:M=> Video)
    public VideoLibrary VideoLibrary { get; set; }
    public int VideoLibraryId { get; set; }

    // section <-1:1=> video
    public Section Section { get; set; }
    public int SectionId { get; set; }
}
