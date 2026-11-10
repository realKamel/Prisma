using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.LessonEvents;

public sealed record LessonPublishedDomainEvent(int LessonId) : BaseDomainEvent;
