using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

public class AcademicYearLessonExistsSpecification : Specification<AcademicYearLesson>
{
    public AcademicYearLessonExistsSpecification(int lessonId, int academicYearId)
    {
        Query.Where(x => x.LessonId == lessonId && x.AcademicYearId == academicYearId);
    }
}