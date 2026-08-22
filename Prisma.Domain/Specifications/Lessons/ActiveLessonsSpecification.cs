using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Lessons;

public class ActiveLessonsSpecification : Specification<Lesson>
{
    public ActiveLessonsSpecification()
    {
        Query
            .Where(l => l.Status == LessonStatus.Active).AsNoTrackingWithIdentityResolution();
    }
    public ActiveLessonsSpecification(Guid? teacherId)
    {
        Query
            .Where(l => l.Status == LessonStatus.Active && l.TeacherId == teacherId)
            .AsNoTrackingWithIdentityResolution();
    }
}