namespace Prisma.Application.Common.Interfaces;

/// <summary>
/// Marker interface to indicate that a request requires a database transaction.
/// Apply this to Command (write) requests.
/// </summary>
public interface ITransactionalRequest { }
