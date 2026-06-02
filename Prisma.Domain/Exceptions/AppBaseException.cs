namespace Prisma.Domain.Exceptions;

public abstract class AppBaseException(string message) : Exception(message)
{
}