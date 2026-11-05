using Ardalis.Result;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Infrastructure.Identity.Extensions;

internal static class IdentityResultExtensions
{
    extension(IdentityResult identityResult)
    {
        public Result ToResult()
        {
            if (identityResult.Succeeded)
            {
                return Result.Success();
            }

            return Result.Invalid(
                identityResult.Errors.Select(error => new ValidationError(
                    identifier: error.Code,
                    errorMessage: error.Description
                ))
            );
        }

        public Result<T> ToResult<T>(T value)
        {
            if (identityResult.Succeeded)
            {
                return Result.Success(value);
            }

            return Result<T>.Invalid(
                identityResult.Errors.Select(error => new ValidationError(
                    identifier: error.Code,
                    errorMessage: error.Description
                ))
            );
        }
    }
}
