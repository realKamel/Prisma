using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.QuizAggregate;

public class LessonQuiz : BaseEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }

    public TimeSpan TimeInMinutes { get; set; }

    public decimal TotalDegree { get; set; }
    public QuizScope Scope { get; set; }
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public int? AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }

    // if null => avail
    public DateTimeOffset? AvailableFrom { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public ICollection<QuestionLessonQuiz> Questions { get; set; } = new List<QuestionLessonQuiz>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}