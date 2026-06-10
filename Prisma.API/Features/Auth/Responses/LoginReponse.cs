using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Authentication.Commands.Login;

namespace Prisma.API.Features.Auth.Responses;

public record LoginApiResponse(Guid? Id, string? Email, string? FirstName, string? SecondName, string? Role);

public static class LoginResponseExtensions
{
    public static LoginApiResponse ToResponse(this Result<LoginResponse> loginResponse)
    {
        var cred = loginResponse.Data.Credentials;
        return new LoginApiResponse(cred.Id, cred.Email, cred.FirstName, cred.SecondName, cred.Role);
    }
}