using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Hybrid;
using IResult = Ardalis.Result.IResult;

namespace Prisma.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute() : TypeFilterAttribute(typeof(IdempotentFilter));

public sealed class IdempotentFilter(HybridCache cache, ILogger<IdempotentFilter> logger)
    : IAsyncActionFilter
{
    private const string HeaderName = "X-Idempotency-Key";

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        ArgumentNullException.ThrowIfNull(context);
        if (
            !context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var rawKey)
            || string.IsNullOrWhiteSpace(rawKey)
        )
        {
            logger.LogMissingHeader(HeaderName);

            context.Result = new BadRequestObjectResult(
                new { error = $"Missing required header: {HeaderName}" }
            );
            return;
        }

        string idempotencyKey = rawKey!;

        string cacheKey = $"idempotency:{idempotencyKey}";

        bool isCacheHit = true;

        var cachedResponse = await cache.GetOrCreateAsync(
            cacheKey,
            state: next,
            async (nextDelegate, _) =>
            {
                isCacheHit = false;
                logger.LogCacheMiss(idempotencyKey);

                var executedContext = await nextDelegate();

                if (executedContext.Result is ObjectResult objectResult)
                {
                    if (objectResult.Value is IResult result)
                    {
                        if (result.Status is ResultStatus.Ok or ResultStatus.Created)
                        {
                            logger.LogResponseCached(idempotencyKey, result.Status.ToString());
                            return new CachedResponse
                            {
                                StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK,
                                Body = objectResult.Value,
                            };
                        }

                        logger.LogBypassingCache(idempotencyKey, result.Status.ToString());
                    }
                    else if (objectResult.StatusCode is >= 200 and < 300)
                    {
                        logger.LogResponseCached(
                            idempotencyKey,
                            objectResult.StatusCode.Value.ToString()
                        );
                        return new CachedResponse
                        {
                            StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK,
                            Body = objectResult.Value,
                        };
                    }
                }

                return null!;
            },
            options: new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(24) },
            cancellationToken: context.HttpContext.RequestAborted
        );

        if (isCacheHit && cachedResponse is not null)
        {
            logger.LogCacheHit(idempotencyKey);

            context.Result = new ObjectResult(cachedResponse.Body)
            {
                StatusCode = cachedResponse.StatusCode,
            };
        }
    }

    private class CachedResponse
    {
        public int StatusCode { get; set; }
        public object? Body { get; set; }
    }
}

/// <summary>
/// Compile-time source-generated logging extensions for IdempotentFilter.
/// </summary>
internal static partial class IdempotentFilterLoggingExtensions
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Warning,
        Message = "Idempotency request rejected: Missing {HeaderName} header"
    )]
    public static partial void LogMissingHeader(this ILogger logger, string headerName);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Idempotency Cache MISS for key {Key}. Processing request..."
    )]
    public static partial void LogCacheMiss(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Information,
        Message = "Idempotency response cached successfully for key {Key} with status {Status}"
    )]
    public static partial void LogResponseCached(this ILogger logger, string key, string status);

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Information,
        Message = "Idempotency bypassing cache for key {Key} due to non-success status: {Status}"
    )]
    public static partial void LogBypassingCache(this ILogger logger, string key, string status);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Information,
        Message = "Idempotency Cache HIT for key {Key}. Returning cached response"
    )]
    public static partial void LogCacheHit(this ILogger logger, string key);
}
