namespace Prisma.Domain.Exceptions;

public class UnauthorizedException(string message = "Unauthorized") :
    AppBaseException(message, "UNAUTHORIZED")
{
}