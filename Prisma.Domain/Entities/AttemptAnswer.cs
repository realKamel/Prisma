using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class AttemptAnswer : BaseEntity
{
    public string? QuizAttemptId { get; set; } 
    public virtual QuizAttempt? QuizAttempt { get; set; }

    public string? StudentId { get; set; } 
    public virtual Student? Student { get; set; }

    public string? QuestionId { get; set; } 
    public virtual Question? Question { get; set; }

    public string? ChoiceId { get; set; } 
    public virtual Choice? Choice { get; set; }

    public decimal Score { get; set; }
    public bool IsCorrect { get; set; }
    public string? TextAnswer { get; set; } 

}