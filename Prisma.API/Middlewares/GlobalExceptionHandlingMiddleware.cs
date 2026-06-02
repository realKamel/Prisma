using Microsoft.AspNetCore.Mvc;
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

            await HandleExceptionAsync(context, ex);
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

        var problemDetails = new ProblemDetails
        {
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path,
            Extensions = { ["errors"] = errors }
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception e)
    {
        var mappedException = MapExceptionToMessage(e);

        context.Response.ContentType = "application/problem+json";

        context.Response.StatusCode = mappedException.code;

        var problemDetails = new ProblemDetails
        {
            Title = mappedException.title,
            Status = mappedException.code,
            Detail = mappedException.details,
            Instance = context.Request.Path
        };
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static (string title, string details, int code ) MapExceptionToMessage(Exception ex)
    {
        (string title, string details, int code ) result;
        switch (ex)
        {
            case OperationCanceledException:
                Log.Warning("Operation timed out");
                result = (title: "Request Timeout", details: "Operation timed out",
                    code: StatusCodes.Status502BadGateway);
                break;

            case UnauthorizedAccessException:
                Log.Warning(ex, "Unauthorized access attempt");
                result = (title: "Unauthorized",
                    details: ex.Message, code: StatusCodes.Status401Unauthorized);
                break;

            case AppBaseException:
                Log.Warning(ex, "Application error: {Message}", ex.Message);
                result = (title: "Bad Request", details: ex.Message, StatusCodes.Status400BadRequest);
                break;

            default:
                Log.Error(ex, "Unhandled exception occurred");
                result = (title: "Internal Server Error",
                    details: "An unexpected error occurred. Please try again later.",
                    code: StatusCodes.Status500InternalServerError);
                break;
        }

        return result;
    }
}