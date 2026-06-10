namespace Prisma.Domain.Exceptions;

public abstract class AppBaseException(string message, string ErrorCode) : Exception(message)
{
}