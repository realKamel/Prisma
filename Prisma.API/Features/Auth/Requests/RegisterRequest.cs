using Prisma.Application.Features.Authentication.Commands.Register;

namespace Prisma.API.Features.Auth.Requests;

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