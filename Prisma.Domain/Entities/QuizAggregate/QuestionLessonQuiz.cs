using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.QuizAggregate;

public class QuestionLessonQuiz : BaseEntity
{
    public int LessonQuizId { get; set; }
    public LessonQuiz LessonQuiz { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; }

    public decimal Degree { get; set; }
}