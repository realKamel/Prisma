namespace Prisma.Domain.Exceptions;

public abstract class AppBaseException(string message, string errorCode) : Exception(message)
{
    public string ErrorCode { get; init; } = errorCode;
}