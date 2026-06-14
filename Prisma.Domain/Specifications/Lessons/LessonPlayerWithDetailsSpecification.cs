using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonPlayerWithDetailsSpecification : Specification<Lesson>
{
    public LessonPlayerWithDetailsSpecification(int lessonId)
        : base()
    {
        Query.Where(lesson => lesson.Id == lessonId)
            .Include(l => l.Sections)
            .ThenInclude(s => s.Progresses)
            .Include(l => l.Quizzes)
            .ThenInclude(q => q.Questions)
            .Include(l => l.Assignments)
            .Include(l => l.Enrollments);

        // AddInclude(l => l.Sections);
        // AddInclude($"{nameof(Lesson.Sections)}.{nameof(Section.Progresses)}");
        // AddInclude(l => l.Quizzes);
        // AddInclude($"{nameof(Lesson.Quizzes)}.{nameof(LessonQuiz.Questions)}");
        // AddInclude(l => l.Assignments);
        // AddInclude(l => l.Enrollments);
    }
}