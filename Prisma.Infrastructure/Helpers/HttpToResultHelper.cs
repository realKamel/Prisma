using System.Net;
using Ardalis.Result;

namespace Prisma.Infrastructure.Helpers;

public static class HttpToResultHelper
{
    public static Result ToResult(this HttpRequestException exception, string servicesName)
    {
        return exception?.StatusCode switch
        {
            HttpStatusCode.BadRequest => Result.Invalid(
                new ValidationError($"The external request to {servicesName} was invalid.")
            ),

            HttpStatusCode.Unauthorized => Result.Unauthorized(),

            HttpStatusCode.Forbidden => Result.Forbidden(),

            HttpStatusCode.NotFound => Result.NotFound(),

            HttpStatusCode.TooManyRequests => Result.Unavailable(
                $"The {servicesName} rate limit was exceeded."
            ),

            HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.BadGateway
            or HttpStatusCode.GatewayTimeout => Result.Unavailable(
                $"The {servicesName} is temporarily unavailable."
            ),

            _ => Result.Error($"The external request to {servicesName} failed."),
        };
    }
}
