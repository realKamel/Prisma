namespace Prisma.API.Features.Auth;

public record RegisterDto(string firstName, string secondName, string thirdName, string lastName, string mobile,
    string email, string password, string confirmPassword, int grade, string parentMobile);
