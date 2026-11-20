using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.Videos;

public sealed record VideoProcessingCompletedDomainEvent(
    Guid EventId,
    int VideoId,
    Guid ProviderVideoId,
    long ProviderVideoLibraryId,
    int? SectionId,
    long DurationInSeconds,
    long StorageSizeBytes,
    Uri? ThumbnailUrl
) : BaseDomainEvent;
