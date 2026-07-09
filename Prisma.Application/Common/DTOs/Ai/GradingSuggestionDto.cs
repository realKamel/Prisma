using System.ComponentModel;

namespace Prisma.Application.Common.DTOs.Ai;

public sealed record GradingSuggestionDto(
    [property: Description("Suggested score out of the maximum possible points for this submission.")]
    decimal SuggestedScore,
    [property: Description("Maximum possible score for this assignment/question.")]
    decimal MaxScore,
    [property:
        Description(
            "Clear, specific rationale explaining why this score was suggested, referencing the student's actual answer.")]
    string Rationale,
    [property:
        Description(
            "Model's self-assessed confidence in this grade, from 0.0 (low) to 1.0 (high). Lower confidence for ambiguous or partially-correct answers.")]
    double Confidence,
    [property: Description("Per-criterion breakdown if the assignment has a rubric. Empty list if ungraded by rubric.")]
    IReadOnlyList<RubricCriterionScore> RubricBreakdown,
    [property:
        Description(
            "Specific concerns worth flagging for teacher review, e.g. possible plagiarism indicators, ambiguous requirements, or answers outside expected scope. Empty if none.")]
    IReadOnlyList<string> FlaggedConcerns);

public sealed record RubricCriterionScore(
    string CriterionName,
    decimal PointsAwarded,
    decimal PointsPossible,
    string Justification);