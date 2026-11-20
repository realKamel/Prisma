using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.DomainEvents.Videos;

public sealed record VideoProcessingFailedDomainEvent(
    Guid EventId,
    int VideoId,
    Guid ProviderVideoId,
    long ProviderVideoLibraryId,
    VideoStatus FailureStatus,
    string? Reason
) : BaseDomainEvent;
