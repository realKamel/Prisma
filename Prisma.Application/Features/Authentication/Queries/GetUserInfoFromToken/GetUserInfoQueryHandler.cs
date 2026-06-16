using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs.Auth;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Features.Authentication.Queries.GetUserInfoFromToken;

public class GetUserInfoQueryHandler(ICurrentUserService currentUserService, UserManager<User> userManager)
    : IRequestHandler<GetUserInfoQuery, Result<LoginCredentials>>
{
    public async Task<Result<LoginCredentials>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.Email is null)
        {
            throw new UnauthorizedException("Login First");
        }

        var user = await userManager.FindByEmailAsync(currentUserService.Email);
        if (user is null)
        {
            throw new UnauthorizedException("Login First");
        }

        var role = await userManager.GetRolesAsync(user);

        var cred = new LoginCredentials(user.Id, user.Email, user.FirstName, user.LastName,
            role.Count > 0 ? role[0] : null);

        return cred;
    }
}