using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Written : BaseEntity
{
    public string? QuestionId { get; set; }
    public virtual Question? Question { get; set; } 

    public string? Text { get; set; }
    public string? Answer { get; set; }

}