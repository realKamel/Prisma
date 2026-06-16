namespace Prisma.Application.Common.DTOs.Auth;

public record LoginCredentials(Guid? Id, string? Email, string? FirstName, string? SecondName, string? Role);