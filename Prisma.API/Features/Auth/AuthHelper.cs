using Prisma.API.Features.Auth.Requests;
using Prisma.Application.Common.DTOs.Auth;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Authentication.Commands.Login;
using Prisma.Application.Features.Authentication.Commands.Register;

namespace Prisma.API.Features.Auth;

public static class AuthHelper
{
    public const string AccessToken = "access_Token";
    public const string RefreshToken = "refresh_token";

    public static void SetAuthCookies(this IResponseCookies responseCookies, string accessToken, string refreshToken)
    {
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true, // JS cannot read it
            Secure = true, // HTTPS only
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(20)
        };

        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/v1/auth/refresh" // TODO: goes to the refresh endpoint, nothing else
        };

        responseCookies.Append(AccessToken, accessToken, accessTokenOptions);
        responseCookies.Append(RefreshToken, refreshToken, refreshTokenOptions);
    }

    public static void RemoveCookies(this IResponseCookies responseCookies)
    {
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true, // JS cannot read it
            Secure = true, // HTTPS only
            SameSite = SameSiteMode.None,
        };

        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Path = "/v1/auth/refresh"
        };
        responseCookies.Delete(AccessToken, accessTokenOptions);
        responseCookies.Delete(RefreshToken, refreshTokenOptions);
    }

    public static Result<LoginCredentials> ToResponse(this Result<LoginResponse> loginResponse)
    {
        var cred = loginResponse.Data.Credentials;
        return Result<LoginCredentials>.Success(cred);
    }

    public static LoginCommand ToCommand(this LoginRequest loginRequest)
    {
        return new LoginCommand(
            loginRequest.Email,
            loginRequest.Mobile,
            loginRequest.Password);
    }

    public static RegisterCommand ToCommand(this RegisterRequest request)
    {
        return new RegisterCommand(
            request.FirstName,
            request.SecondName,
            request.ThirdName,
            request.LastName,
            request.Mobile,
            request.Email,
            request.Password,
            request.ConfirmPassword,
            request.Grade,
            request.ParentMobile);
    }
}