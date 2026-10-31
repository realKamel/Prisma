namespace Prisma.Application.Abstractions.Services.VideoServices;

public sealed record VideoLibraryCreateResult(
    long ExternalLibraryId,
    string ApiKey,
    string ReadOnlyApiKey,
    string? WebhookUrl
);
