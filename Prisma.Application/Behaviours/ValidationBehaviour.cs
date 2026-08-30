using System.Reflection;
using Ardalis.Result;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Prisma.Application.Behaviours;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        List<ValidationFailure> failures =
        [
            .. validationResults
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null),
        ];

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        List<ValidationError> validationErrors =
        [
            .. failures.Select(f => new ValidationError(
                f.PropertyName,
                f.ErrorMessage,
                f.ErrorCode,
                MapSeverity(f.Severity)
            )),
        ];

        Type responseType = typeof(TResponse);

        // If the handler returns Result<T>, return an Invalid result through the pipeline.
        if (
            responseType.IsGenericType
            && responseType.GetGenericTypeDefinition() == typeof(Result<>)
        )
        {
            Type resultType = responseType.GetGenericArguments()[0];

            MethodInfo invalidMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethods()
                .First(m =>
                    m.Name == nameof(Result<>.Invalid)
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(IEnumerable<ValidationError>)
                );
            return (TResponse)invalidMethod.Invoke(null, [validationErrors])!;
        }

        // If the handler returns plain Result, return Invalid as well.
        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Invalid(validationErrors);
        }

        // Otherwise, keep throwing so the global exception middleware handles it.
        throw new ValidationException(failures);
    }

    private static ValidationSeverity MapSeverity(Severity severity)
    {
        return severity switch
        {
            Severity.Error => ValidationSeverity.Error,
            Severity.Warning => ValidationSeverity.Warning,
            Severity.Info => ValidationSeverity.Info,
            _ => ValidationSeverity.Error,
        };
    }
}
