using Prisma.Domain.Common;

namespace Prisma.Domain.Aggregates;

public abstract class AggregateRoot : BaseEntity<Guid>
{
    protected AggregateRoot()
        : base() { }

    protected AggregateRoot(Guid id)
    {
        this.Id = id;
    }
}
