using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Services;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.RealTime;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    public static void AddSignalRWithConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionStrings = configuration
            .GetSection(ConnectionStringsOptions.SectionName)
            .Get<ConnectionStringsOptions>();

        ArgumentNullException.ThrowIfNull(connectionStrings);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionStrings.Valkey,
            nameof(connectionStrings.Valkey)
        );

        services.AddSignalR().AddStackExchangeRedis(connectionStrings.Valkey);

        services.AddSingleton<IUserIdProvider, AppUserIdProvider>();
        services.AddSingleton<IChatPresenceTracker, RedisChatPresenceTracker>();
        services.AddSingleton<IUnreadBadgeStore, RedisUnreadBadgeStore>();
    }
}