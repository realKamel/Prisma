using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.QuizAggregate;

public class TrueFalseQuestion : Question
{
    public bool IsCorrect { get; set; }
}
