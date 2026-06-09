using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Auth;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user;

        if (request.Email is null)
        {
            user = await userManager
                .Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone, cancellationToken);
        }
        else
        {
            user = await userManager.FindByEmailAsync(request.Email);
        }

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Result<LoginResponse>.Failure("Invalid credentials.");

        var roles = (await userManager.GetRolesAsync(user)).ToList();

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email, roles);

        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;

        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return Result<LoginResponse>.Success(new LoginResponse(accessToken, refreshToken, roles));
    }
}