using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Authentication.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string PhoneNumber,
    string Email,
    string Password,
    string ConfirmPassword,
    int AcademicYear,
    string ParentPhoneNumber) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(string accessToken, string refreshToken);