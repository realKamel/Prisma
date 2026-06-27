using MediatR;

namespace Prisma.Application.Features.TeacherStudents.Commands.UpdateStudent;

public record UpdateStudentCommand(
    Guid StudentId,
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    string Email,
    string? NewPassword,
    int Grade,
    string? ParentMobile
) : IRequest<bool>;