using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.QuizAggregate;

public class Choice : BaseEntity
{
    public int QuestionId { get; set; }
    public Question Question { get; set; }

    public string? Text { get; set; }
    public bool IsCorrect { get; set; }
}