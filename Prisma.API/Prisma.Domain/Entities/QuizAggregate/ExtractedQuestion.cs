using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.QuizAggregate;

public class ExtractedQuestion
{
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public List<string> Options { get; set; } = new();
    public int? CorrectIndex { get; set; }
    public bool? CorrectBool { get; set; }
    public string ModelAnswer { get; set; } = string.Empty;
    public int Score { get; set; } = 10;
}
