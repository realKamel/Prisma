using MediatR;

namespace Prisma.Application.Abstractions.Messaging;

/// <summary>
/// Defines a handler for a command that performs a state-changing operation without returning a value.
/// </summary>
/// <typeparam name="TRequest">The type of command being handled. Must implement <see cref="ICommand"/>.</typeparam>
public interface ICommandHandler<in TRequest> : IRequestHandler<TRequest>
    where TRequest : ICommand
{
    // Inherits Handle method implementation requirements from MediatR's IRequestHandler.
}

/// <summary>
/// Defines a handler for a command that performs a state-changing operation and returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The type of command being handled. Must implement <see cref="ICommand{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of data returned after the command execution (e.g., a newly created ID).</typeparam>
public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    // Inherits Handle method implementation requirements from MediatR's IRequestHandler.
}
