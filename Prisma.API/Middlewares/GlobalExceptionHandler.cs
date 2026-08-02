using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Prisma.API.Middlewares;

/// <summary>
/// Global safety net that converts any unhandled exception into a generic RFC 9457
/// Problem Details response without leaking sensitive information.
/// </summary>
/// <remarks>
/// <para>
/// Expected application failures are communicated via <c>Ardalis.Result</c> values and
/// translated to Problem Details by the Ardalis result convention
/// (<see cref="Ardalis.Result.AspNetCore.TranslateResultToActionResultAttribute"/> +
/// <c>AddDefaultResultConvention()</c>). The application intentionally does NOT throw
/// exceptions for expected domain errors, so anything that reaches this handler is a bug,
/// a cancellation, or a framework-level request failure.
/// </para>
/// <para>
/// Because this handler is a safety net, its contract is:
/// </para>
/// <list type="bullet">
/// <item><description>Never disclose internal exception messages, stack traces, or other sensitive details to the client.</description></item>
/// <item><description>Return a generic <c>500 Internal Server Error</c> body for unexpected failures, while logging the full exception server-side for diagnosis.</description></item>
/// <item><description>Map only framework-level, non-sensitive conditions to actionable status codes (validation, timeouts, malformed requests).</description></item>
/// </list>
/// <para>Exception-to-status mapping:</para>
/// <list type="bullet">
/// <item><description><see cref="FluentValidation.ValidationException"/> → <c>400 Bad Request</c> plus a per-property <c>errors</c> extension (validation that escaped the pipeline).</description></item>
/// <item><description><see cref="OperationCanceledException"/> caused by a server-side timeout → <c>504 Gateway Timeout</c>. Client-aborted requests are swallowed without a response.</description></item>
/// <item><description><see cref="BadHttpRequestException"/> → the exception's own status code (malformed JSON, unsupported media type, payload too large, ...).</description></item>
/// <item><description>Anything else → <c>500 Internal Server Error</c> with a generic message; the real reason is only logged, never sent to the client.</description></item>
/// </list>
/// <para>
/// The <c>traceId</c> extension is added by <c>AddProblemDetails()</c>
/// (see <c>WebAppHelper.AddWebAppServices</c>) so clients can correlate failures with
/// server-side logs.
/// </para>
/// </remarks>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        // The client disconnected — writing a response would be pointless (and the current
        // connection is gone).
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug(exception,
                "Client aborted request {Method} {Path} (TraceId: {TraceId}).",
                httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);

            return true;
        }

        // Headers/body already flushed — we cannot replace the response at this point.
        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning(exception,
                "Response already started for {Method} {Path}; cannot write Problem Details (TraceId: {TraceId}).",
                httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);

            return false;
        }

        var (status, title, detail, extra) = MapException(exception);

        // Unexpected 500s carry the full stack trace in logs; handled conditions are
        // expected behavior and logged at debug level. Nothing here reaches the client.
        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception,
                "Unhandled exception processing {Method} {Path} (TraceId: {TraceId}).",
                httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogDebug(exception,
                "Handled {ExceptionType} for {Method} {Path} -> {StatusCode} (TraceId: {TraceId}).",
                exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path,
                status, httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Title = title, Detail = detail, Status = status, Instance = httpContext.Request.Path,
        };

        extra?.Invoke(problem);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext, ProblemDetails = problem, Exception = exception,
        });
    }

    /// <summary>
    /// Maps an exception to the Problem Details fields it should produce.
    /// </summary>
    private static (int Status, string Title, string Detail, Action<ProblemDetails>? Extra) MapException(
        Exception exception) => exception switch
    {
        FluentValidation.ValidationException vex => (
            StatusCodes.Status400BadRequest,
            "Validation Failed",
            "One or more validation errors occurred.",
            p =>
            {
                p.Extensions["errors"] = vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            }),

        // Server-side timeout only — client aborts are filtered out at the top
        // of TryHandleAsync.
        OperationCanceledException => (
            StatusCodes.Status504GatewayTimeout,
            "Operation Timed Out",
            "The operation timed out. Please try again.",
            null),

        // Malformed requests rejected by the server (invalid content type,
        // malformed JSON, payload too large, ...). The exception message describes the
        // request problem, not application internals.
        BadHttpRequestException badHttp => (
            badHttp.StatusCode,
            GetReasonTitle(badHttp.StatusCode),
            badHttp.Message,
            null),

        // Fallback for bugs and unexpected failures: the client only ever sees a generic
        // message — never the real exception message or stack trace.
        _ => (
            StatusCodes.Status500InternalServerError,
            "An error occurred while processing your request.",
            "An unexpected error occurred. Please try again later.",
            null),
    };

    private static string GetReasonTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status413PayloadTooLarge => "Payload Too Large",
        StatusCodes.Status415UnsupportedMediaType => "Unsupported Media Type",
        _ => "Request Error",
    };
}