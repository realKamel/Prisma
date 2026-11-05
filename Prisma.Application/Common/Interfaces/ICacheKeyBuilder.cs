namespace Prisma.Application.Common.Interfaces;

public interface ICacheKeyBuilder
{
    //global cache key builder for all cacheable queries
    string Build(string key);

    //cache per user -- this throw if user isn't logged in
    string ForUser(string key);

    // string ForWorkspace(string key);
}
