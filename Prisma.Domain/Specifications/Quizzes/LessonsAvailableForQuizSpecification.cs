using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class LessonsAvailableForQuizSpecification : Specification<Lesson>
{
    public LessonsAvailableForQuizSpecification()
    {
        Query.Where(l => l.QuizId == null);
    }
}
