using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonWithDetailsSpecification : Specification<Lesson>
{
    public LessonWithDetailsSpecification(int lessonId)
        : base()
    {
        Query.Where(lesson => lesson.Id == lessonId)
            .Include(l => l.Sections)
            .Include(l => l.Enrollments)
            .Include(l => l.Quizzes)
            .ThenInclude(q => q.Attempts)
            .Include(l => l.Prerequisite)
            .ThenInclude(p => p.Quizzes)
            .ThenInclude(q => q.Attempts);

        // AddCriteria(l => l.Id == lessonId);
        // AddInclude(l => l.Sections);
        // AddInclude(l => l.Enrollments);
        // AddInclude(l => l.Quizzes);
        // AddInclude($"{nameof(Lesson.Quizzes)}.{nameof(LessonQuiz.Attempts)}");
        // AddInclude(l => l.Prerequisite);
        // AddInclude($"{nameof(Lesson.Prerequisite)}.{nameof(Lesson.Quizzes)}");
        // AddInclude($"{nameof(Lesson.Prerequisite)}.{nameof(Lesson.Quizzes)}.{nameof(LessonQuiz.Attempts)}");
    }
}