using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class ActiveLessonsOfTeacherSpec : Specification<Lesson>
{
    public ActiveLessonsOfTeacherSpec(Guid teacherId)
    {
        Query.Where(l => l.TeacherId == teacherId && l.Status == LessonStatus.Active);
    }
}
