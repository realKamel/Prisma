using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class TeacherQuizzesSpecification: Specification<Quiz>
{
    public TeacherQuizzesSpecification(QuizScope scope, string? search)
    {
        Query
            .Where(q => q.Scope == scope)
            .Include(q => q.Questions)
            .Include(q => q.Attempts);

        if (!string.IsNullOrWhiteSpace(search))
            Query.Where(q => q.Title != null && q.Title.Contains(search));

    }
}
