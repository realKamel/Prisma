using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.API.Features.Auth.Requests;
using Prisma.Application.Features.Authentication.Commands.ForgotPassword;
using Prisma.Application.Features.Authentication.Commands.Logout;
using Prisma.Application.Features.Authentication.Commands.RefreshToken;

namespace Prisma.API.Features.Auth;

public class AuthController(IMediator mediator) : ApiController
{
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request, CancellationToken cancelToken)
    {
        var result = await mediator.Send(request.ToCommand(), cancelToken);
        Response.Cookies.SetAuthCookies(result.Data?.accessToken, result.Data?.refreshToken);
        return Ok();
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancelToken)
    {
        var result = await mediator.Send(request.ToCommand(), cancellationToken: cancelToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken(CancellationToken cancelToken)
    {
        var command = new
            RefreshTokenCommand(Request.Cookies["accessToken"],
                Request.Cookies["refreshToken"]);
        var result = await mediator.Send(command, cancelToken);
        Response.Cookies.SetAuthCookies(result.Data?.AccessToken, result.Data?.RefreshToken);
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout(CancellationToken cancelToken)
    {
        var result = await mediator.Send(new LogoutCommand(), cancelToken);

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return Ok();
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