namespace Prisma.Domain.Common;

public interface IDomainEventEnumerable
{
    IReadOnlyCollection<BaseDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
