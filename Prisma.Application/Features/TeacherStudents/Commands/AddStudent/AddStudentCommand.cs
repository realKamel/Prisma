using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.TeacherStudents.Commands.AddStudent;

public record AddStudentCommand(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    string Email,
    string Password,
    int Grade,
    string ParentMobile) : IRequest<Result>;
