namespace Prisma.Domain.Exceptions;

public class BadRequestException(string message) : AppBaseException(message, "BAD_REQUEST")
{
}