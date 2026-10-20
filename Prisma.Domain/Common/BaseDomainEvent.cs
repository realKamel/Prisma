namespace Prisma.Domain.Common;

public abstract record BaseDomainEvent
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
