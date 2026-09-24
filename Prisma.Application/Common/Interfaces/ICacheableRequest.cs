namespace Prisma.Application.Common.Interfaces;

public interface ICacheableRequest<TResponse>
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}