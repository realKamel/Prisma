using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
namespace Prisma.Domain.Specifications.Lessons;

public class LessonByIdSpecification: Specification<Lesson>
{
    public LessonByIdSpecification(int lessonId)
    {
        Query.Where(l => l.Id == lessonId);
    }
}
