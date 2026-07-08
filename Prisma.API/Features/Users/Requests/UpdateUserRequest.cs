namespace Prisma.API.Features.Users.Requests;

public record UpdateUserRequest(
    string FirstName, string SecondName, string ThirdName, string LastName,
    string Mobile, string Email, string? NewPassword,
    int? GradeId, Guid? TeacherId, string? ParentMobile);