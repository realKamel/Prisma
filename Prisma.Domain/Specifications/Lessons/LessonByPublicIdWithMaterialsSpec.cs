using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class LessonByPublicIdWithMaterialsSpec : Specification<Lesson>
{
    public LessonByPublicIdWithMaterialsSpec(Guid lessonId)
    {
        Query.Where(lesson => lesson.PublicId == lessonId).Include(l => l.LessonMaterials);
    }
}
