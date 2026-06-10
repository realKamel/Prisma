namespace Prisma.Domain.Exceptions;

public class ConflictException(string message) : AppBaseException(message, "CONFLICT");