using MediatR;

namespace Prisma.Application.Abstractions.Messaging;

/// <summary>
/// Defines a handler for a query that returns a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The type of query being handled. Must implement <see cref="IQuery{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of data returned by the query execution.</typeparam>
public interface IQueryHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IQuery<TResponse>
{
    // Inherits Handle method implementation requirements from MediatR's IRequestHandler.
}
