namespace Prisma.Application.Common.DTOs;

public sealed record FileDto(string FileName, string ContentType, long Size);
