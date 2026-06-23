using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;

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
            throw new BadRequestException("Invalid credentials");

        var roles = (await userManager.GetRolesAsync(user)).ToList();

        var permissions = await userManager.GetClaimsAsync(user);

        var accessToken = jwtTokenService
            .GenerateAccessToken(user.Id, user.Email, roles, permissions);

        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;

        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return Result<LoginResponse>.Success(
            new LoginResponse(accessToken, refreshToken,
                new(user.Id,
                    user.Email,
                    user.FirstName,
                    user.SecondName,
                    roles.Count == 0 ? null : roles[0])));
    }
}