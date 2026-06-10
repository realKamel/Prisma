using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Auth;
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

        if (user is not Student student || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new BadRequestException("Invalid credentials");

        var roles = (await userManager.GetRolesAsync(student)).ToList();

        var accessToken = jwtTokenService.GenerateAccessToken(student.Id, student.Email, roles);

        var refreshToken = jwtTokenService.GenerateRefreshToken();

        student.RefreshToken = refreshToken;

        student.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

        await userManager.UpdateAsync(student);

        return Result<LoginResponse>.Success(
            new LoginResponse(accessToken, refreshToken,
                new(student.Id,
                    student.Email,
                    student.FirstName,
                    student.SecondName,
                    roles[0])));
    }
}