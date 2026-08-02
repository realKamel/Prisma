using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.API.Features.Auth.Requests;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs.Auth;
using Ardalis.Result;
using Prisma.Application.Features.Authentication.Commands.EmailVerification;
using Prisma.Application.Features.Authentication.Commands.ForgotPassword;
using Prisma.Application.Features.Authentication.Commands.Logout;
using Prisma.Application.Features.Authentication.Commands.RefreshToken;
using Prisma.Application.Features.Authentication.Queries.GetUserInfoFromToken;

namespace Prisma.API.Features.Auth;

public class AuthController(IMediator mediator, IWebHostEnvironment environment) : ApiController
{
    [HttpPost("login")]
    public async Task<Result<LoginCredentials>> Login([FromBody] LoginRequest request, CancellationToken cancelToken)
    {
        var result = await mediator.Send(request.ToCommand(),
            cancelToken);

        Response.Cookies.SetAuthCookies(result.Value.AccessToken, result.Value.RefreshToken,
            environment.IsDevelopment());

        return result.Map(r => new LoginCredentials(r.Credentials.Id, r.Credentials.Email, r.Credentials.FirstName,
            r.Credentials.SecondName, r.Credentials.Role, r.Credentials.Permissions));
    }

    [HttpPost("register")]
    public async Task<Result> Register([FromBody] RegisterRequest request, CancellationToken cancelToken)
    {
        return await mediator.Send(request.ToCommand(), cancellationToken: cancelToken);
    }

    [HttpPost("refresh")]
    public async Task<Result<AuthResponse>> RefreshToken(CancellationToken cancelToken)
    {
        var accessToken = Request.Cookies[AppCookies.AccessToken];
        var refreshToken = Request.Cookies[AppCookies.RefreshToken];

        var command = new
            RefreshTokenCommand(accessToken, refreshToken);

        var result = await mediator.Send(command, cancelToken);

        Response.Cookies.SetAuthCookies(result.Value.AccessToken, result.Value.RefreshToken);

        return result;
    }

    [HttpPost("logout")]
    public async Task<Result> Logout(CancellationToken cancelToken)
    {
        var result = await mediator.Send(new LogoutCommand(Request.Cookies[AppCookies.AccessToken]), cancelToken);

        Response.Cookies.RemoveCookies(environment.IsDevelopment());

        return result;
    }


    [HttpPost("forgot-password")]
    public async Task<Result> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return result;
    }

    [HttpPost("confirm-code")]
    public async Task<Result> ConfirmCode([FromBody] ConfirmCodeCommand command)
    {
        var result = await mediator.Send(command);

        return result;
    }

    [HttpPost("reset-password")]
    public async Task<Result> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await mediator.Send(command);

        return result;
    }

    [HttpPost("email-verify")]
    public async Task<Result> EmailVerify([FromBody] EmailVerificationRequestCommand command)
    {
        var result = await mediator.Send(command);

        return result;
    }

    [HttpGet("confirm-email")]
    public async Task<ActionResult> ConfirmEmail([FromQuery] ConfirmEmailCommand command)
    {
        var result = await mediator.Send(command);
        return Redirect("/login");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<Result<LoginCredentials>> GetUserInfo(CancellationToken cancelToken)
    {
        return await mediator.Send(new GetUserInfoQuery(), cancelToken);
    }
}