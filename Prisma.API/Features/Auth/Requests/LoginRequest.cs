using Prisma.Application.Features.Authentication.Commands.Login;

namespace Prisma.API.Features.Auth.Requests;

public record LoginRequest(string? Email, string Password, string? Mobile);

public static class LoginRequestMapper
{
    public static LoginCommand ToCommand(this LoginRequest loginRequest)
    {
        return new LoginCommand(
            loginRequest.Email,
            loginRequest.Mobile,
            loginRequest.Password);
    }
}