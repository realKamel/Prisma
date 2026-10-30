namespace Prisma.Domain.Exceptions;

public class EmailDeliveryException(string errors) : Exception(errors) { }
