using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.LessonEvents;

public record SectionRemovedVideoEvent(int LessonId, int SectionId, int VideoId) : BaseDomainEvent;
