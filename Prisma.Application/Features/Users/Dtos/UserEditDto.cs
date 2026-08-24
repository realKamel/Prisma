namespace Prisma.Application.Features.Users.Dtos;

public record UserEditDto
(
    Guid Id,
    string FirstName,
    string? SecondName,
    string? ThirdName,
    string LastName,
    string? Mobile,
    string? Email,
    string Role,
    int? GradeId,
    List<string> TeacherIds,
    string? ParentMobile
    );