namespace Prisma.Application.Abstractions.VideoServices;

public sealed record VideoLibraryCreateResult(
    long ExternalLibraryId,
    string ApiKey,
    string ReadOnlyApiKey,
    string? WebhookUrl
);
