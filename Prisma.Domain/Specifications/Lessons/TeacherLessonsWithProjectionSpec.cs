using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class TeacherLessonsWithProjectionSpec<TResult> : Specification<Lesson, TResult>
{
    public TeacherLessonsWithProjectionSpec(Guid teacherId, Expression<Func<Lesson, TResult>> projection)
    {
        Query
            .Where(l => l.TeacherId == teacherId)
            .AsNoTracking()
            .Select(projection);
    }
}