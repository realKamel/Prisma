namespace Prisma.API.Features.Users.Requests;

public record CreateUserRequest(
    string FirstName, string SecondName, string ThirdName, string LastName,
    string Mobile, string Email, string Password, string Role,
    int? GradeId, Guid? TeacherId, string? ParentMobile, string? Subject);
