using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs.Auth;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService,
    ITokenService tokenService
) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await identityService.FindByEmailOrPhoneAsync(
            request.Email,
            request.Phone,
            cancellationToken
        );

        if (user is null || !await identityService.CheckPasswordAsync(user, request.Password))
        {
            return Result.Unauthorized("COMMON.UNAUTHORIZED");
        }

        if (user is Teacher teacher)
        {
            var suspensionError = CheckTeacherSuspension(teacher);
            if (suspensionError is not null)
            {
                return suspensionError;
            }
        }

        if (user is Assistant assistant)
        {
            var teacherOfAssistant = await identityService.FindByIdAsync(
                assistant.TeacherId.Value,
                cancellationToken
            );
            if (teacherOfAssistant is Teacher parentTeacher)
            {
                var suspensionError = CheckTeacherSuspension(parentTeacher);
                if (suspensionError is not null)
                {
                    return suspensionError;
                }
            }
        }

        var roles = user.Roles.Select(x => x.Role.Name).ToList();

        var permissions = user
            .Claims.Where(c => c is not null)
            .Select(c => new Claim(c.ClaimType, c.ClaimValue))
            .ToList();

        var permissionList = permissions.Select(c => c.Value).ToArray();

        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            roles,
            permissions
        );

        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.MarkAsOnline();

        await tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return new LoginResponse(
            accessToken,
            refreshToken,
            new LoginCredentials(
                user.Id,
                user.Email,
                user.FirstName,
                user.SecondName,
                Role: roles.FirstOrDefault(),
                permissionList
            )
        );
    }

    private static Result? CheckTeacherSuspension(Teacher teacher)
    {
        if (teacher.Status != TeacherStatus.Suspended)
        {
            return null;
        }

        var message = string.IsNullOrEmpty(teacher.SuspensionReason)
            ? "تم إيقاف حسابك، برجاء التواصل مع الدعم الفني"
            : $"تم إيقاف حسابك: {teacher.SuspensionReason}";

        return Result.Unauthorized(message);
    }
}
