using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Choice : BaseEntity
{
    public required string QuestionId { get; set; }
    public required virtual Question Question { get; set; } 

    public string? Text { get; set; } 
    public bool IsCorrect { get; set; }

}