namespace Prisma.Application.Abstractions.Ai;

public interface IGradingAssistant
{
    Task<GradingSuggestionResult> SuggestGradeAsync(
        string studentAnswer, string modelAnswer, int maxScore, CancellationToken ct);
}

public sealed record GradingSuggestionResult(int SuggestedScore, string Rationale);