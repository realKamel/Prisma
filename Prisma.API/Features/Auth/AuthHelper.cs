namespace Prisma.API.Features.Auth;

public static class AuthHelper
{
    public static void SetAuthCookies(this IResponseCookies responseCookies, string accessToken, string refreshToken)
    {
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true, // JS cannot read it
            Secure = true, // HTTPS only
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "https://localhost:7109/api/v1/Auth/refresh" // TODO: goes to the refresh endpoint, nothing else
        };
        responseCookies.Append("access_token", accessToken, accessTokenOptions);
        responseCookies.Append("refresh_token", refreshToken, refreshTokenOptions);
    }
}