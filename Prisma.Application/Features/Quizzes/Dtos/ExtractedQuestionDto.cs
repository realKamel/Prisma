using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class ExtractedQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public decimal Degree { get; set; }
    public List<ExtractedChoiceDto>? Choices { get; set; }
    public string? ModelAnswer { get; set; }
    public bool? IsCorrect { get; set; }
}

public class ExtractedChoiceDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
