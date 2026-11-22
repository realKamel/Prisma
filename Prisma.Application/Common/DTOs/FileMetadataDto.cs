namespace Prisma.Application.Common.DTOs;

public sealed record FileMetadataDto(string FileName, string ContentType, long Size);
