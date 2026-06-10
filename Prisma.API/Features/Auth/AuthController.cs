using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prisma.API.Common;
using Prisma.API.Features.Auth.Requests;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Authentication.Commands.ForgotPassword;
using Prisma.Application.Features.Authentication.Commands.Login;
using Prisma.Application.Features.Authentication.Commands.Logout;
using Prisma.Application.Features.Authentication.Commands.RefreshToken;
using Prisma.Application.Features.Authentication.Commands.Register;

namespace Prisma.API.Features.Auth;

public class AuthController(IMediator mediator) : ApiController
{
    [HttpPost("login")]
    [ProducesResponseType<Result<LoginCredentials>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<LoginCredentials>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<LoginCredentials>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Login([FromBody] LoginRequest request, CancellationToken cancelToken)
    {
        var result = await mediator.Send(request.ToCommand(), cancelToken);
        Response.Cookies.SetAuthCookies(result.Data.AccessToken, result.Data.RefreshToken);
        return Ok(result.ToResponse());
    }

    [HttpPost("register")]
    [ProducesResponseType<Result<RegisterCommand>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<RegisterCommand>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<LoginCredentials>>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancelToken)
    {
        var result = await mediator.Send(request.ToCommand(), cancellationToken: cancelToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken(CancellationToken cancelToken)
    {
        var command = new
            RefreshTokenCommand(Request.Cookies["accessToken"],
                Request.Cookies["refreshToken"]);

        var result = await mediator.Send(command, cancelToken);

        Response.Cookies.SetAuthCookies(result.Data.AccessToken, result.Data.RefreshToken);

        return Ok();
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout(CancellationToken cancelToken)
    {
        await mediator.Send(new LogoutCommand(), cancelToken);

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return Ok();
    }


    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await mediator.Send(command);
  
        return Ok(result);
    }

    [HttpPost("confirm-code")]
    public async Task<ActionResult> ConfirmCode([FromBody] ConfirmCodeCommand command)
    {
        var result = await mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await mediator.Send(command);

        return Ok(result);
    }
}