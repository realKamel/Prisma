using Prisma.Domain.Common;

namespace Prisma.Domain.DomainEvents.ObjectStorage;

public sealed record FileObjectUploaded(Guid WorkspaceId, Guid ObjectId) : BaseDomainEvent;
