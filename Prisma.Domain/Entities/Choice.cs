using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Choice : BaseEntity
{
    public int QuestionId { get; set; }
    public Question? Question { get; set; } 

    public string? Text { get; set; } 
    public bool IsCorrect { get; set; }

}