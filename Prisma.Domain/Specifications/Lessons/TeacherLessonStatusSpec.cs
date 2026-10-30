using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Lessons;

public class TeacherLessonStatusesSpec : Specification<Lesson, LessonStatus>
{
    public TeacherLessonStatusesSpec(Guid teacherId)
    {
        Query.Where(l => l.TeacherId == teacherId).AsNoTracking().Select(l => l.Status);
    }
}