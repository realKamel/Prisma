namespace Prisma.API.Features.Auth.Requests;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);