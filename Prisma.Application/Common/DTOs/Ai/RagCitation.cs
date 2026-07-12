namespace Prisma.Application.Common.DTOs.Ai;

public sealed record RagCitation(
    string SourceId,
    string SourceTitle,
    string Excerpt);