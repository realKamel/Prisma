using Prisma.Domain.Common;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class AttemptAnswer : BaseEntity
{
    public int QuizAttemptId { get; set; } 
    public QuizAttempt? QuizAttempt { get; set; }

    public Guid StudentId { get; set; } 
    public  Student? Student { get; set; }

    public int QuestionId { get; set; } 
    public Question? Question { get; set; }

    public int ChoiceId { get; set; } 
    public Choice? Choice { get; set; }

    public decimal Score { get; set; }
    public bool IsCorrect { get; set; }
    public string? TextAnswer { get; set; } 

}