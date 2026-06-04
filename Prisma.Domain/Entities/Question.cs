using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public abstract class Question : BaseEntity
{
    public string? Title { get; set; } 

    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();
    public virtual ICollection<QuestionLessonQuiz> QuestionLessons { get; set; } = new List<QuestionLessonQuiz>();
}
