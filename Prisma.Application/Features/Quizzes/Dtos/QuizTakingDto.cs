using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class QuizTakingDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public string? Subject { get; set; }
    public int DurationMinutes { get; set; }
    public int RemainingSeconds { get; set; }
    public List<QuizQuestionTakingDto> Questions { get; set; } = new();
}

public class QuizQuestionTakingDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public decimal Degree { get; set; }

    // null for Written questions
    public List<QuizChoiceDto>? Choices { get; set; }

    // save answer if student return to quiz
    public int? SelectedChoiceId { get; set; }
    public string? SavedTextAnswer { get; set; }
}

public class QuizChoiceDto
{
    public int ChoiceId { get; set; }
    public string Text { get; set; } = string.Empty;
}

