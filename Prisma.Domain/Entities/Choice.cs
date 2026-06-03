using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Choice : BaseEntity
{
    public string? QuestionId { get; set; }
    public virtual Question? Question { get; set; } 

    public string? Text { get; set; } 
    public bool IsCorrect { get; set; }

}