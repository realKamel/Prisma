using MediatR;

namespace Prisma.Application.Common.Interfaces;

public interface IBaseAuthorizableRequest
{
    IReadOnlyList<string> RequiredRoles { get; }

    IReadOnlyList<string> RequiredPermissions { get; }
}

public interface IAuthorizableRequest<out TResponse>
    : IRequest<TResponse>,
        IBaseAuthorizableRequest { }

public interface IAuthorizableRequest : IRequest, IBaseAuthorizableRequest { }
