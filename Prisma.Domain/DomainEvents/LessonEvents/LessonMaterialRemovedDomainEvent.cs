using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.LessonEvents;

public record LessonMaterialRemovedDomainEvent(Guid FileObjectId) : BaseDomainEvent;
