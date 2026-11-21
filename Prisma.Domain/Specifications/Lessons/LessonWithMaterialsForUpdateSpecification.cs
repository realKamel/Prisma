using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class LessonWithMaterialsForUpdateSpecification : Specification<Lesson>
{
    public LessonWithMaterialsForUpdateSpecification(int lessonId)
    {
        Query.Where(lesson => lesson.Id == lessonId).Include(l => l.LessonMaterials);
    }

    public LessonWithMaterialsForUpdateSpecification(Guid lessonId)
    {
        Query.Where(lesson => lesson.PublicId == lessonId).Include(l => l.LessonMaterials);
    }
}
