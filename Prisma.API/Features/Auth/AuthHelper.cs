using Prisma.API.Features.Auth.Requests;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs.Auth;
using Ardalis.Result;
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
            // Lax for localhost
            SameSite = SameSiteMode.Lax,
            // 5 minutes window to be used after it's expiry for refresh mechanism
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        var refreshTokenOptions = new CookieOptions
        {
            Path = "/api/v1/auth/refresh",
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
        };

        responseCookies.Append(AppCookies.AccessToken, accessToken, accessTokenOptions);
        responseCookies.Append(AppCookies.RefreshToken, refreshToken, refreshTokenOptions);
    }

    public static void RemoveCookies(this IResponseCookies responseCookies, bool isDevelopment)
    {
        var accessTokenOptions = new CookieOptions
        {
            Path = "/api", HttpOnly = true, Secure = !isDevelopment, SameSite = SameSiteMode.Lax
        };

        var refreshTokenOptions = new CookieOptions
        {
            Path = "/api/v1/auth/refresh", HttpOnly = true, Secure = !isDevelopment, SameSite = SameSiteMode.Lax
        };

        responseCookies.Delete(AppCookies.AccessToken, accessTokenOptions);
        responseCookies.Delete(AppCookies.RefreshToken, refreshTokenOptions);
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