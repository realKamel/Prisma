using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.Application.Features.Authentication.Commands.ForgotPassword;
using Prisma.Application.Features.Authentication.Commands.Login;
using Prisma.Application.Features.Authentication.Commands.RefreshToken;
using Prisma.Application.Features.Authentication.Commands.Register;

namespace Prisma.API.Features.Auth;

public class AuthController(IMediator mediator) : ApiController
{
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginCommand command, CancellationToken cancelToken)
    {
        var result = await mediator.Send(command, cancelToken);
        SetAuthCookies(result.Data?.accessToken, result.Data?.refreshToken);
        return Ok();
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancelToken)
    {
        var result = await mediator.Send(command,
            cancellationToken: cancelToken);
        SetAuthCookies(result.Data?.accessToken, result.Data?.refreshToken);
        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken(CancellationToken cancelToken)
    {
        var command = new RefreshTokenCommand(Request.Cookies["accessToken"],
            Request.Cookies["refreshToken"]);

        var result = await mediator.Send(command, cancelToken);

        SetAuthCookies(result.Data?.AccessToken, result.Data?.RefreshToken);

        return Ok();
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout(CancellationToken cancelToken)
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        return Ok();
    }

    private void SetAuthCookies(string accessToken, string refreshToken)
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
        Response.Cookies.Append("access_token", accessToken, accessTokenOptions);
        Response.Cookies.Append("refresh_token", refreshToken, refreshTokenOptions);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("confirm-code")]
    public async Task<ActionResult> ConfirmCode(ConfirmCodeCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }
}