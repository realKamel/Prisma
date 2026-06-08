using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Entities.QuizAggregate;

public class LessonQuiz : BaseEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }

    public TimeSpan TimeInMinutes { get; set; }

    public decimal TotalDegree { get; set; }
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public virtual ICollection<QuestionLessonQuiz> Questions { get; set; } = new List<QuestionLessonQuiz>();
    public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}