namespace Prisma.Application.Features.Lessons.Dtos;

internal sealed record LibraryMainInfoReadModel(
    int VideoLibraryId,
    long? ProviderLibraryId,
    string? LibraryApiKey
);
