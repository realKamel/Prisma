namespace Prisma.Domain.Exceptions;

public class NotFoundException(string entityName, object key) : AppBaseException(
    $"{entityName} with id '{key}' was not found",
    "NOT_FOUND")
{
}