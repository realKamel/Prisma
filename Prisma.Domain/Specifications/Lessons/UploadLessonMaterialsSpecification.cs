using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Teachers;

public class UploadLessonMaterialsSpecification : Specification<Lesson>
{
    public UploadLessonMaterialsSpecification(int lessonId)
    {
        Query.Where(lesson => lesson.Id == lessonId && !lesson.IsDeleted)
             .Include(l => l.LessonMaterials);
    }
}