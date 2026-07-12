namespace Prisma.Application.Common.DTOs.Ai;

public sealed record AnswerResult(
    string Answer,
    IReadOnlyList<RagCitation> Citations,
    double Confidence);