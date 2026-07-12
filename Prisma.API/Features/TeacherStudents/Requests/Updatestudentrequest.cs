namespace Prisma.API.Features.TeacherStudents.Requests;

public record UpdateStudentRequest(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    string Email,
    string? NewPassword,
    int Grade,
    string? ParentMobile
);