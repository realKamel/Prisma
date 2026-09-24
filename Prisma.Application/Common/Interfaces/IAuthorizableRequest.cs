namespace Prisma.Application.Common.Interfaces;

public interface IAuthorizableRequest
{
    IList<string> RequiredRoles { get; }
    IList<string> RequiredPermissions { get; }
}
