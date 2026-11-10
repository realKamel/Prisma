namespace Prisma.Application.Abstractions.Email;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    Guid IdempotencyKey,
    string? TextBody = null
);
