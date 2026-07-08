namespace Prisma.Application.Features.Users.Dtos;

public record UserListItemDto
(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool Active,
    string Joined,
    string LastActive
    );