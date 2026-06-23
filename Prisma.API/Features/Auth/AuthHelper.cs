using Prisma.API.Features.Auth.Requests;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs.Auth;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Authentication.Commands.Login;
using Prisma.Application.Features.Authentication.Commands.Register;

namespace Prisma.API.Features.Auth;

public static class AuthHelper
{
    public static void SetAuthCookies(this IResponseCookies responseCookies,
        string accessToken,
        string refreshToken,
        bool isDevelopment = false)
    {
        var accessTokenOptions = new CookieOptions
        {
            Path = "/api",
            HttpOnly = true, // JS cannot read it
            Secure = !isDevelopment, //  this for dev
            // Lax for localhost, None for cross - domain prod
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            // 5 minutes window to be used after it's expiry for refresh mechanism
            Expires = DateTimeOffset.UtcNow.AddMinutes(20)
        };

        var refreshTokenOptions = new CookieOptions
        {
            Path = "/api", // TODO: goes to the refresh endpoint, nothing else
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
        };

        responseCookies.Append(AppCookies.AccessToken, accessToken, accessTokenOptions);
        responseCookies.Append(AppCookies.RefreshToken, refreshToken, refreshTokenOptions);
    }

    public static void RemoveCookies(this IResponseCookies responseCookies, bool isDevelopment)
    {
        var accessTokenOptions = new CookieOptions
        {
            Path = "/api",
            HttpOnly = true, // JS cannot read it
            Secure = !isDevelopment, //  this for dev
            // Lax for localhost, None for cross - domain prod
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
        };

        var refreshTokenOptions = new CookieOptions
        {
            Path = "/api", // TODO: goes to the refresh endpoint, nothing else
            HttpOnly = true,
            Secure = !isDevelopment, //  this for dev
            // Lax for localhost, None for cross - domain prod
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
        };
        responseCookies.Delete(AppCookies.AccessToken, accessTokenOptions);
        responseCookies.Delete(AppCookies.RefreshToken, refreshTokenOptions);
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