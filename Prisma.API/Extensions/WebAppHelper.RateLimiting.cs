using System.Security.Claims;
using System.Threading.RateLimiting;
using Prisma.API.Common.RateLimitConfigurations;
using RedisRateLimiting;
using StackExchange.Redis;

namespace Prisma.API.Extensions;

public static partial class WebAppHelper
{
    private static void AddRateLimiterConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>()
                       ?? throw new InvalidOperationException("RateLimiting configuration is missing.");

        services.AddRateLimiter((options) =>
        {
            //clients get 429
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Policy 1: AUTH endpoints (login, register, refresh)
            // Strict: 10 attempts / min / IP, No brute force & credential stuffing.
            // Sliding window = precise rolling enforcement, no window-edge bursts.
            options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
            {
                var multiplexer = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                    partitionKey: $"rl:auth:ip:{httpContext.Connection.RemoteIpAddress}",
                    factory: _ => new RedisSlidingWindowRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => multiplexer,
                        PermitLimit = settings.Auth.MaxRequestsPerMinute,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            //Policy 2: PUBLIC endpoints (catalogs, etc.)
            // Token bucket per IP: 100 req/min with burst tolerance.
            options.AddPolicy(RateLimitPolicies.Public, httpContext =>
            {
                var multiplexer = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                return RedisRateLimitPartition.GetTokenBucketRateLimiter(
                    partitionKey: $"rl:public:ip:{httpContext.Connection.RemoteIpAddress}",
                    factory: _ => new RedisTokenBucketRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => multiplexer,
                        TokenLimit = settings.Public.BurstLimit, // Max instant capacity
                        TokensPerPeriod = settings.Public.SustainedPerMinute, // Refill rate
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1)
                    });
            });

            // Policy 3: PROTECTED endpoints (authenticated users)
            // Per USER ID (from JWT), not per IP — logged-in users get their own bucket.
            options.AddPolicy(RateLimitPolicies.UserRead, httpContext =>
            {
                var multiplexer = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                return RedisRateLimitPartition.GetTokenBucketRateLimiter(
                    partitionKey: $"rl:user:read:{httpContext.User.FindFirst("sub")?.Value ?? "anon"}",
                    factory: _ => new RedisTokenBucketRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => multiplexer,
                        TokenLimit = settings.User.ReadBurstLimit,
                        TokensPerPeriod = settings.User.ReadSustainedPerMinute,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1)
                    });
            });


            // 4. USER WRITE: Token Bucket (Strict for costly operations like DB inserts/LLM)
            options.AddPolicy(RateLimitPolicies.UserWrite, httpContext =>
            {
                var redis = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                return RedisRateLimitPartition.GetTokenBucketRateLimiter(
                    partitionKey: $"rl:user:write:{httpContext.User.FindFirst("sub")?.Value ?? "anon"}",
                    factory: _ => new RedisTokenBucketRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => redis,
                        TokenLimit = settings.User.WriteBurstLimit,
                        TokensPerPeriod = settings.User.WriteSustainedPerMinute,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1)
                    });
            });
            // Policy 5: LLM STREAMING endpoints
            // Concurrency limit: Max 2 simultaneous streaming requests per user.
            // QueueLimit = 0 ensures we fail fast (429) instead of making the user wait in a queue 
            // while their other streams are still running.
            options.AddPolicy(RateLimitPolicies.LlmConcurrency, httpContext =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? httpContext.User.FindFirstValue("sub")
                             ?? "anon";

                var multiplexer = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                return RedisRateLimitPartition.GetConcurrencyRateLimiter(
                    partitionKey: $"rl:llm:concurrent:{userId}",
                    factory: _ => new RedisConcurrencyRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => multiplexer,
                        PermitLimit = settings.Llm.MaxConcurrentStreams,
                        QueueLimit = 0 // CRITICAL: Reject immediately, do not queue LLM requests
                    });
            });

            // Shared 429 response with Retry-After + problem+json
            options.OnRejected = async (context, ct) =>
            {
                var response = context.HttpContext.Response;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                response.StatusCode = 429;
                response.ContentType = "application/problem+json";

                await response.WriteAsJsonAsync(
                    new
                    {
                        type = "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
                        title = "Too Many Requests",
                        status = 429,
                        detail =
                            "Rate limit exceeded. Please slow down and retry after the duration specified in the Retry-After header.",
                        traceId = context.HttpContext.TraceIdentifier
                    }, cancellationToken: ct);
            };
        });
    }
}