namespace Prisma.Domain.Common;

public class BaseEntity<TKey> : IEntity<TKey>, IAuditable, IDomainEventEnumerable
{
    public TKey Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    //event dispatching
    private readonly List<BaseDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(BaseDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public class BaseEntity : BaseEntity<int> { }
