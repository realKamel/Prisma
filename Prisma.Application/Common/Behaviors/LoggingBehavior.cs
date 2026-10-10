using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).Name;
        var requestId = Guid.CreateVersion7().ToString("N")[..8];

        // 1. Source-generated start log
        logger.LogRequestStarting(requestId, requestName, request);

        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            var response = await next(cancellationToken);

            var elapsedMs = (long)Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            // 2. Source-generated success log
            logger.LogRequestSuccess(requestId, requestName, elapsedMs);

            // 3. Source-generated slow request warning
            if (elapsedMs > 500)
            {
                logger.LogRequestSlow(requestId, requestName, elapsedMs);
            }

            return response;
        }
        catch (Exception ex)
        {
            var elapsedMs = (long)Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            logger.LogRequestFailure(ex, requestId, requestName, elapsedMs, ex.Message);

            throw;
        }
    }
}

/// <summary>
///  compile-time source-generated logging extensions.
/// </summary>
internal static partial class LoggingBehaviorExtensions
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "[{RequestId}] Handling {RequestName} {@Request}"
    )]
    public static partial void LogRequestStarting(
        this ILogger logger,
        string requestId,
        string requestName,
        object request
    );

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "[{RequestId}] Handled {RequestName} successfully in {ElapsedMs}ms"
    )]
    public static partial void LogRequestSuccess(
        this ILogger logger,
        string requestId,
        string requestName,
        long elapsedMs
    );

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "[{RequestId}] PERFORMANCE WARNING: {RequestName} took {ElapsedMs}ms"
    )]
    public static partial void LogRequestSlow(
        this ILogger logger,
        string requestId,
        string requestName,
        long elapsedMs
    );

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "[{RequestId}] FAILED {RequestName} after {ElapsedMs}ms due to {ErrorMessage}"
    )]
    public static partial void LogRequestFailure(
        this ILogger logger,
        Exception ex,
        string requestId,
        string requestName,
        long elapsedMs,
        string errorMessage
    );
}
