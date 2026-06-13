using Prisma.Application.Common.Responses;
using Prisma.Domain.Exceptions;
using Serilog;

namespace Prisma.API.Middlewares;

public class GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (FluentValidation.ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failed for request {Path}", context.Request.Path);
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (OperationCanceledException ex)
        {
            if (context.RequestAborted.IsCancellationRequested)
            {
                // Client disconnected => don't treat as error
                logger.LogDebug("Client disconnected on {Path}", context.Request.Path);
                context.Abort();
                return;
            }

            logger.LogWarning(ex, "Operation cancelled on {Path}", context.Request.Path);

            await HandleCancelledOperationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context,
        FluentValidation.ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var response = Result.ValidationFailure(errors);

        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception e)
    {
        var mappedException = MapExceptionToMessage(e);

        context.Response.ContentType = "application/problem+json";

        context.Response.StatusCode = mappedException.code;

        var response = Result.Failure(mappedException.details);

        await context.Response.WriteAsJsonAsync(response);
    }

    private static (string title, string details, int code ) MapExceptionToMessage(Exception ex)
    {
        (string title, string details, int code ) result;
        if (ex is not AppBaseException exception)
        {
            Log.Error(ex, "Unhandled exception occurred");
            return (title: "Internal Server Error",
                details: "An unexpected error occurred. Please try again later.",
                code: StatusCodes.Status500InternalServerError);
        }
        else
        {
            switch (exception)
            {
                case BadRequestException:
                    Log.Warning(exception, exception.Message);
                    result = (exception.ErrorCode, details: exception.Message,
                        code: StatusCodes.Status400BadRequest);
                    break;

                case UnauthorizedException:
                    Log.Warning(exception, "Unauthorized access attempt");
                    result = (title: exception.ErrorCode,
                        details: exception.Message, code: StatusCodes.Status401Unauthorized);
                    break;

                case not null:
                    Log.Warning(exception, "Application error: {Message}", exception.Message);
                    result = (title: exception.ErrorCode, details: exception.Message, StatusCodes.Status400BadRequest);
                    break;

                default:
                    Log.Error(exception, "Unhandled exception occurred");
                    result = (title: "Internal Server Error",
                        details: "An unexpected error occurred. Please try again later.",
                        code: StatusCodes.Status500InternalServerError);
                    break;
            }
        }

        return result;
    }

    private static async Task HandleCancelledOperationExceptionAsync(HttpContext context, Exception e)
    {
        Log.Warning("Operation timed out");
        var mappedException = (title: "Request Timeout", details: "Operation timed out",
            code: StatusCodes.Status502BadGateway);

        context.Response.ContentType = "application/problem+json";

        context.Response.StatusCode = mappedException.code;

        var response = Result.Failure(mappedException.title);

        await context.Response.WriteAsJsonAsync(response);
    }
}