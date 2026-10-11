using MediatR;

namespace Prisma.Domain.Common;

public abstract record BaseDomainEvent : INotification
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
