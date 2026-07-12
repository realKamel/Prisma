namespace Prisma.API.Features.TeacherStudents.Requests;

public record AddStudentRequest(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Mobile,
    string Email,
    string Password,
    int Grade,
    string ParentMobile);
