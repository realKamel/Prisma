namespace Prisma.Domain.Exceptions;

public class ForbiddenException(string message = "Forbidden") : AppBaseException(message, "FORBIDDEN")
{
}