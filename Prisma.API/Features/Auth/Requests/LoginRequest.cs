using Prisma.Application.Features.Authentication.Commands.Login;

namespace Prisma.API.Features.Auth.Requests;

public record LoginRequest(string? Email, string Password, string? Mobile);
