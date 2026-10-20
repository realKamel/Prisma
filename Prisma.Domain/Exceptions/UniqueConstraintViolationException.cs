namespace Prisma.Domain.Exceptions;

public sealed class UniqueConstraintViolationException(Exception inner)
    : Exception("A unique constraint was violated.", inner);
