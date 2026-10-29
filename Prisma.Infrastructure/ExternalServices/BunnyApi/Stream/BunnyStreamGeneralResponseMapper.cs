using Ardalis.Result;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Stream;

internal static class BunnyStreamGeneralResponseMapper
{
    public static Result ToResult(this BunnyStreamGeneralResponse response)
    {
        if (response.Success)
        {
            return Result.Success();
        }

        var result = response.StatusCode switch
        {
            401 => Result.Unauthorized(),
            403 => Result.Forbidden(),
            404 => Result.NotFound(),
            500 => Result.Error(response.Message),
            _ => Result.Error(),
        };
        return result;
    }
}
