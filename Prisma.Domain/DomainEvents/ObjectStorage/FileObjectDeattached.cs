using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.ObjectStorage;

public sealed record FileObjectRemoved(Guid ObjectStorageId, Guid FileObjectId) : BaseDomainEvent;
