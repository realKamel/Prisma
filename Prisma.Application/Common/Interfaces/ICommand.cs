using MediatR;

namespace Prisma.Application.Common.Interfaces;

/// <summary>
/// Marker interface to represent a command in the application.
/// Use this for write operations that may require handling with side effects.
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Generic Marker interface to represent a command in the application.
/// Use this for write operations that may require handling with side effects.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}