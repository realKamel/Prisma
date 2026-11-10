namespace Prisma.Application.Abstractions.Authorization;

public interface IAuthorizableRequest
{
    IReadOnlyList<string> RequiredRoles { get; }

    IReadOnlyList<string> RequiredPermissions { get; }
}
