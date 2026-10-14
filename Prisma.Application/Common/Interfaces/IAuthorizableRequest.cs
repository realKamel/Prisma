using MediatR;

namespace Prisma.Application.Common.Interfaces;

public interface IAuthorizableRequest<out TResponse> : IRequest<TResponse>
{
    IReadOnlyList<string> RequiredRoles { get; }
    IReadOnlyList<string> RequiredPermissions { get; }
}

public interface IAuthorizableRequest : IRequest
{
    IReadOnlyList<string> RequiredRoles { get; }
    IReadOnlyList<string> RequiredPermissions { get; }
}