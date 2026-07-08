using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    string Email,
    string? NewPassword,
    int? GradeId,
    Guid? TeacherId,
    string? ParentMobile
) : IRequest<Result<UserEditDto>>;