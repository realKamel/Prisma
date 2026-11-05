using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs.Auth;

namespace Prisma.Application.Features.Authentication.Queries.GetUserInfoFromToken;

public sealed class GetUserInfoQueryHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService
) : IRequestHandler<GetUserInfoQuery, Result<LoginCredentials>>
{
    public async Task<Result<LoginCredentials>> Handle(
        GetUserInfoQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is not { } userId)
        {
            return Result.Unauthorized();
        }

        var user = await identityService.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.Unauthorized("User isn't found");
        }

        var roles = await identityService.GetRolesAsync(user);

        var permissions = (await identityService.GetClaimsAsync(user))
            .Select(claim => claim.Value)
            .ToArray();

        return new LoginCredentials(
            user.Id,
            user.Email,
            user.FirstName,
            user.SecondName,
            roles.FirstOrDefault(),
            permissions
        );
    }
}
