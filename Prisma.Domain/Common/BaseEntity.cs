namespace Prisma.Domain.Common;

public class BaseEntity<TKey>
    : IEntity<TKey>,
        IAuditable,
        IDomainEventEnumerable,
        IEquatable<BaseEntity<TKey>>
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

    public bool Equals(BaseEntity<TKey>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // Optional: ensure runtime types match (handles EF Core proxies correctly)
        if (GetType() != other.GetType())
        {
            return false;
        }

        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseEntity<TKey>);
    }

    public static bool operator ==(BaseEntity<TKey>? left, BaseEntity<TKey>? right)
    {
        if (left is null)
        {
            return right is null; // Returns true if both are null, false if only left is null
        }

        return left.Equals(right); // Safe because 'left' is guaranteed not to be null here
    }

    public static bool operator !=(BaseEntity<TKey>? left, BaseEntity<TKey>? right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        // EqualityComparer to safely handle potential null IDs (e.g., if TKey is a string)
        return EqualityComparer<TKey>.Default.GetHashCode(Id!);
    }
}

public class BaseEntity : BaseEntity<int> { }
