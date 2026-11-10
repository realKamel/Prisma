using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.LessonEvents;

public sealed record LessonAssignmentRemoved(int AssignmentId, Guid FileObjectId) : BaseDomainEvent;
