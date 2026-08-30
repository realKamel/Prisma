using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;

namespace Prisma.API.Filters;

internal sealed class LocalizeResultAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var localizer = context.HttpContext.RequestServices.GetRequiredService<IAppLocalizer>();

        //Intercept Raw Validation Errors
        if (
            context.Result is BadRequestObjectResult badRequest
            && badRequest.Value is IEnumerable<ValidationError> validationErrors
        )
        {
            var localizedErrors = validationErrors.Select(v => new
            {
                field = v.Identifier,
                code = v.ErrorCode, // was v.ErrorMessage
                message = !string.IsNullOrWhiteSpace(v.ErrorCode)
                    ? localizer[v.ErrorCode]
                    : v.ErrorMessage, // fallback
            });

            context.Result = new BadRequestObjectResult(new { errors = localizedErrors });
            return;
        }

        // 2. Intercept ValidationProblemDetails (Ardalis 400 Bad Request Output)
        if (
            context.Result is ObjectResult valResult
            && valResult.Value is ValidationProblemDetails valProblemDetails
        )
        {
            var localizedDictionary = new Dictionary<string, string[]>();

            foreach (var (field, errorCodes) in valProblemDetails.Errors)
            {
                var translatedMessages = errorCodes.Select(code => localizer[code]).ToArray();

                localizedDictionary.Add(field, translatedMessages);
            }

            valProblemDetails.Title = localizer[ErrorKeys.Common.BadRequest];
            valProblemDetails.Errors.Clear();
            foreach (var kvp in localizedDictionary)
            {
                valProblemDetails.Errors.Add(kvp.Key, kvp.Value);
            }

            return;
        }

        // 3. Intercept Standard ProblemDetails (404, 422, 500)
        if (
            context.Result is ObjectResult objectResult
            && objectResult.Value is ProblemDetails problemDetails
        )
        {
            if (!string.IsNullOrEmpty(problemDetails.Detail))
            {
                problemDetails.Detail = localizer[problemDetails.Detail];
            }

            if (!string.IsNullOrEmpty(problemDetails.Title))
            {
                problemDetails.Title = localizer[problemDetails.Title];
            }
        }
    }
}
