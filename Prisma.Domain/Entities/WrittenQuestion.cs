namespace Prisma.Domain.Entities;

public class WrittenQuestion : Question
{
    public virtual ICollection<Written> WrittenOptions { get; set; } = new List<Written>();

}