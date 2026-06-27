namespace Prisma.API.Features.TeacherStudents.Requests;

public record UpdateStudentRequest(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    int Grade,
    string? ParentMobile
);