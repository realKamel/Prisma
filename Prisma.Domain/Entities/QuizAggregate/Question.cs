using Prisma.Domain.Common;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.QuizAggregate;

public abstract class Question : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();
    public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}