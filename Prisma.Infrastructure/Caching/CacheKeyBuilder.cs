using Prisma.Application.Abstractions.Caching;
using Prisma.Application.Abstractions.Identity;

namespace Prisma.Infrastructure.Caching;

internal sealed class CacheKeyBuilder(ICurrentUserService currentUser) : ICacheKeyBuilder
{
    public string ForPublic(string key) => key;

    public string ForUser(string key)
    {
        EnsureAuthenticated();

        return $"{key}:user:{currentUser.UserId}";
    }

    // public string ForWorkspace(string key)
    // {
    //     EnsureAuthenticated();
    //
    //     return $"{key}:workspace:{currentUser.WorkspaceId}";
    // }

    private void EnsureAuthenticated()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new InvalidOperationException("An authenticated user is required.");
        }
    }
}
