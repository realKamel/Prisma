using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.LessonAggregate;

public class GradingSuggestion : BaseEntity
{
    public Guid SubmissionId { get; private set; }
    public decimal SuggestedScore { get; private set; }
    public decimal MaxScore { get; private set; }
    public string Rationale { get; private set; } = null!;
    public double Confidence { get; private set; }
    public GradingSuggestionStatus Status { get; private set; }
    public decimal? FinalScore { get; private set; }
    public Guid? ReviewedByTeacherId { get; private set; }
    public string? TeacherNote { get; private set; }

    private GradingSuggestion()
    {
    }

    public static GradingSuggestion CreateFromAiSuggestion(
        Guid submissionId, decimal suggestedScore, decimal maxScore, string rationale, double confidence)
        => new()
        {
            SubmissionId = submissionId,
            SuggestedScore = suggestedScore,
            MaxScore = maxScore,
            Rationale = rationale,
            Confidence = confidence,
            Status = GradingSuggestionStatus.Suggested
        };

    public bool Approve(Guid teacherId)
    {
        if (Status != GradingSuggestionStatus.Suggested)
            return false;

        Status = GradingSuggestionStatus.Approved;
        FinalScore = SuggestedScore;
        ReviewedByTeacherId = teacherId;
        return true;
    }

    public bool Override(Guid teacherId, decimal finalScore, string note)
    {
        if (Status != GradingSuggestionStatus.Suggested)
            return false;

        Status = GradingSuggestionStatus.Modified;
        FinalScore = finalScore;
        ReviewedByTeacherId = teacherId;
        TeacherNote = note;
        return true;
    }
}