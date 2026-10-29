using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents;

public sealed record TeacherCreatedEvent(Guid TeacherId) : BaseDomainEvent;
