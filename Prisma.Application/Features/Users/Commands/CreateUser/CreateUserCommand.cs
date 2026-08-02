using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;

namespace Prisma.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    string Email,
    string Password,
    string Role,           // "Admin" | "Teacher" | "Student" | "Assistant"
    int? GradeId,           // Student only
    Guid? TeacherId,        // Student only — ignored for Assistant, see handler note
    string? ParentMobile    // Student only
) : IRequest<Result<UserEditDto>>;