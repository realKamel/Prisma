using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler(ICurrentUserService currentUserService, UserManager<User> userManager)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var email = currentUserService.Email;

        if (email is null)
        {
            return Result.Failure("Something went wrong");
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Result.Failure("Something went wrong");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await userManager.UpdateAsync(user);

        return Result.Success();
    }
}