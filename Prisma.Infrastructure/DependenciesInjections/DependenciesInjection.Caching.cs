using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Infrastructure.Persistence;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Prisma.Infrastructure.DependenciesInjections;

public static partial class DependenciesInjection
{
    private static void AddCacheServices(
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

        services
            .AddDataProtection()
            .PersistKeysToStackExchangeRedis(
                ConnectionMultiplexer.Connect(connectionStrings.Valkey),
                "DataProtection-Keys"
            );

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionStrings.Valkey;
        });

        services
            .AddFusionCache("prisma_cache")
            .WithDefaultEntryOptions(options =>
            {
                // General Cache Duration
                options.Duration = TimeSpan.FromMinutes(10);

                // A. Fail-Safe: If DB crashes under load, we serve stale exam data
                // up to 6 (will change) hours instead of throwing an HTTP 500 error to students.
                options.IsFailSafeEnabled = true;
                options.FailSafeMaxDuration = TimeSpan.FromHours(24);
                options.FailSafeThrottleDuration = TimeSpan.FromSeconds(30);

                // B. Soft Timeout: If DB takes longer than 150ms during a rush,
                // abort waiting and instantly entry from the cached payload.
                options.FactorySoftTimeout = TimeSpan.FromMilliseconds(150);

                // C. Hard Timeout: Never allow a DB call to block an API thread
                // for longer than 2 seconds.
                options.FactoryHardTimeout = TimeSpan.FromSeconds(2);

                // Dynamic Jittering: random extra seconds to expiration times.
                // Prevents thousands of cache entries from expiring simultaneously.
                options.JitterMaxDuration = TimeSpan.FromSeconds(30);
            })
            // Serializer for L2 Valkey Cache
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>())
            // Backplane: Syncs all API nodes so no server returns old data
            .WithBackplane(
                new RedisBackplane(
                    new RedisBackplaneOptions { Configuration = connectionStrings.Valkey }
                )
            )
            .AsHybridCache();
    }
}
