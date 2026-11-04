namespace Prisma.Domain.Exceptions;

public class ConcurrencyConflictException()
    : ConflictException("تم تعديل هذا السجل بواسطة عملية أخرى، يرجى المحاولة مرة أخرى");
