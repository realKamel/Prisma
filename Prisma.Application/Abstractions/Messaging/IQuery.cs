using MediatR;

namespace Prisma.Application.Abstractions.Messaging;

/// <summary>
/// Represents a query that does not return a value in a CQRS pattern.
/// </summary>
public interface IQuery : IRequest { }

/// <summary>
/// Represents a query that returns a response of type <typeparamref name="TResponse"/> in a CQRS pattern.
/// </summary>
/// <typeparam name="TResponse">The type of data returned when the query is executed.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse> { }
