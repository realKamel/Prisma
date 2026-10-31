namespace Prisma.Application.Features.Lessons.Dtos;

public sealed record LibraryMainInfoReadModel(
    int VideoLibraryId,
    long? ProviderLibraryId,
    string? LibraryApiKey
);
