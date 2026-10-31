namespace Prisma.API.Features.Auth.Requests;

public sealed record LoginRequest(string? Email, string Password, string? Mobile);
