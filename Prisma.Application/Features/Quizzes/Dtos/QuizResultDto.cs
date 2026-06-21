using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class QuizResultDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "pending" | "done"
    public decimal? Score { get; set; }
    public decimal TotalDegree { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int PendingCount { get; set; }
    public DateTimeOffset? GradedAt { get; set; }
    public DateTimeOffset? AvailableAt { get; set; }

    public int TabSwitchCount { get; set; }
    public int CopyPasteAttemptCount { get; set; }


    // null لو Status == "pending"
    public List<QuizReviewQuestionDto>? Review { get; set; }
}

public class QuizReviewQuestionDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }

    public List<QuizReviewChoiceDto>? Choices { get; set; }
    public int? SelectedChoiceId { get; set; }

    public string? TextAnswer { get; set; }
    public string? CorrectWrittenAnswer { get; set; }

    public bool? IsCorrect { get; set; }
    public decimal? Score { get; set; }
    public decimal Degree { get; set; }
}

public class QuizReviewChoiceDto
{
    public int ChoiceId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
