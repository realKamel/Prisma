namespace Prisma.Domain.Entities;

public class MCQQuestion : Question
{
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();

}
