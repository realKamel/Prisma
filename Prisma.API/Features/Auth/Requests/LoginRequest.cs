namespace Prisma.API.Features.Auth.Requests;

public record LoginRequest(string Email, string Password);