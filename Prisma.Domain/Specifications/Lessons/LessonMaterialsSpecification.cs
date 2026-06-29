using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Teachers;

public class LessonMaterialsSpecification : Specification<Lesson>
{
    public LessonMaterialsSpecification(int lessonId)
    {
        Query.Where(lesson => lesson.Id == lessonId && !lesson.IsDeleted)
             .Include(l => l.LessonMaterials);
    }
}