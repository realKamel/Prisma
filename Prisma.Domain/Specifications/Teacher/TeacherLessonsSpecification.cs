using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Teachers;

public class TeacherLessonsSpecification : Specification<Lesson>
{
    public TeacherLessonsSpecification()
    {
        Query
            .Include(l => l.Enrollments)
            .OrderByDescending(l => l.Id)
            .AsSplitQuery()
            .AsNoTracking();
    }
}