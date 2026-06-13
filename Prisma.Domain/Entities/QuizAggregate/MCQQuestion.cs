namespace Prisma.Domain.Entities.QuizAggregate;

public class MCQQuestion : Question
{
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
}