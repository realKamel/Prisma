namespace Prisma.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInAnyRole(IReadOnlyList<string>? role, CancellationToken cancellationToken = default);
}