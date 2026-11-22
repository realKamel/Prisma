namespace Prisma.Domain.Common;

public interface IEntity<TKey>
    where TKey : struct
{
    TKey Id { get; init; }
}
