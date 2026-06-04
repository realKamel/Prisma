namespace Prisma.Domain.Entities;

public class MCQQuestion : Question
{
    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();

}
