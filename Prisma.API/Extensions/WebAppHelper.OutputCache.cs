using Prisma.Application.Common.Constants;

namespace Prisma.API.Extensions;

public static partial class WebAppHelper
{
    private static void AddOutputCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOutputCache(options =>
        {
            //  Default policy for ALL endpoints
            options.AddBasePolicy(builder =>
                builder.Expire(TimeSpan.FromSeconds(10)));

            // Named policies
            options.AddPolicy(CachePolicyNames.Short.Name, builder =>
                builder.Expire(CachePolicyNames.Short.Duration));

            options.AddPolicy(CachePolicyNames.Long.Name, builder =>
                builder.Expire(CachePolicyNames.Long.Duration));
        });

        services.AddStackExchangeRedisOutputCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Valkey");
            options.InstanceName = "Prisma_OutputCache_";
        });
    }
}