using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class QuizStructureDto
{
    public int QuizId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public int DurationMinutes { get; init; }
    public List<QuizQuestionStructureDto> Questions { get; init; } = [];
}

public class QuizQuestionStructureDto
{
    public int QuestionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public QuestionType Type { get; init; }
    public decimal Degree { get; init; }
    public List<QuizChoiceDto>? Choices { get; init; }
}
