namespace Prisma.Application.Abstractions.Caching;

public interface ICacheKeyBuilder
{
    //global cache key builder for all cacheable queries
    string ForPublic(string key);

    //cache per user -- this throws if user isn't logged in
    string ForUser(string key);

    // string ForWorkspace(string key);
}
