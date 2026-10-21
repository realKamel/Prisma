using System.Reflection;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Common.Behaviors;

internal sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserService currentUser,
    IIdentityService identityService
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuthorizableRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        bool requiresAuthorization =
            request.RequiredRoles.Count > 0 || request.RequiredPermissions.Count > 0;

        if (requiresAuthorization && !currentUser.UserId.HasValue)
        {
            return CreateFailure(
                nameof(Result.Unauthorized),
                "Must Be Logged in Frist, Login and Try Again."
            );
        }

        if (
            request.RequiredRoles.Count > 0
            && !currentUser.IsInAnyRole(request.RequiredRoles, cancellationToken)
        )
        {
            return CreateFailure(
                nameof(Result.Forbidden),
                "User does not have the required role or permissions."
            );
        }

        if (request.RequiredPermissions.Count > 0)
        {
            var hasPermissions = await identityService.HasPermissionsAsync(
                currentUser.UserId!.Value,
                request.RequiredPermissions,
                cancellationToken
            );

            if (!hasPermissions.IsSuccess)
            {
                return CreateFailure(
                    nameof(Result.Forbidden),
                    "User does not have the required permissions."
                );
            }
        }

        return await next(cancellationToken);
    }

    private static TResponse CreateFailure(string methodName, string message)
    {
        Type responseType = typeof(TResponse);

        // Result<T>
        if (
            responseType.IsGenericType
            && responseType.GetGenericTypeDefinition() == typeof(Result<>)
        )
        {
            return (TResponse)InvokeResultFactory(responseType, methodName, message);
        }

        // Result
        if (responseType == typeof(Result))
        {
            return (TResponse)InvokeResultFactory(responseType, methodName, message);
        }

        // Preserve exception-based handling for non-Result responses.
        if (methodName == nameof(Result.Unauthorized))
        {
            throw new UnauthorizedAccessException(message);
        }

        throw new UnauthorizedAccessException(message);
    }

    private static object InvokeResultFactory(Type responseType, string methodName, string message)
    {
        MethodInfo factory = responseType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(method =>
                method.Name == methodName
                && method.GetParameters().Length == 1
                && method.GetParameters()[0].ParameterType == typeof(string)
            );

        return factory.Invoke(null, [message])!;
    }
}
