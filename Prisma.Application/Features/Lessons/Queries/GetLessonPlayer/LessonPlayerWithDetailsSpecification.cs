using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Specifications;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;

public class LessonPlayerWithDetailsSpecification : BaseSpecification<Lesson>
{
    public LessonPlayerWithDetailsSpecification(int lessonId)
        : base()
    {
        AddInclude(l => l.Sections);
        AddInclude($"{nameof(Lesson.Sections)}.{nameof(Section.Progresses)}");
        AddInclude(l => l.Quizzes);
        AddInclude($"{nameof(Lesson.Quizzes)}.{nameof(LessonQuiz.Questions)}");
        AddInclude(l => l.Assignments);
        AddInclude(l => l.Enrollments);
    }
}