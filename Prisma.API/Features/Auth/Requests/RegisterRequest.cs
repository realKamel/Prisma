using Prisma.Application.Features.Authentication.Commands.Register;

namespace Prisma.API.Features.Auth;

public record RegisterRequest(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    string Email,
    string Password,
    string ConfirmPassword,
    int Grade,
    string ParentMobile);

public static class RegisterRequestMapping
{
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