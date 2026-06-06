using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class LessonQuiz : BaseEntity
{
    public string? Title { get; set; } 
    public string? Description { get; set; }
    public int TimeByMinutes { get; set; } 
    public decimal TotalDegree { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public virtual ICollection<QuestionLessonQuiz> Questions { get; set; } = new List<QuestionLessonQuiz>();
    public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}