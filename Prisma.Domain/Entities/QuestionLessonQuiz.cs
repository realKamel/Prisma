using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class QuestionLessonQuiz : BaseEntity
{
    public string? LessonQuizId { get; set; }
    public virtual LessonQuiz? LessonQuiz { get; set; } 

    public string? QuestionId { get; set; }
    public virtual Question? Question { get; set; }

    public decimal Degree { get; set; }

}