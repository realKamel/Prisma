namespace Prisma.Application.Abstractions.Services.Email;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    Guid IdempotencyKey,
    string? TextBody = null
);
