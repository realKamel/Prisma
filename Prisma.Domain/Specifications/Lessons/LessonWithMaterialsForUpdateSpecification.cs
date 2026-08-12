using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonWithMaterialsForUpdateSpecification : Specification<Lesson>
{
    public LessonWithMaterialsForUpdateSpecification(int lessonId)
    {
        Query.Where(lesson => lesson.Id == lessonId && !lesson.IsDeleted)
             .Include(l => l.LessonMaterials);
    }
}