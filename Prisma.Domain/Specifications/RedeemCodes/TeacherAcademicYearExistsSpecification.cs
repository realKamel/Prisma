using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

public class TeacherAcademicYearExistsSpecification : Specification<AcademicYearTeacher>
{
    public TeacherAcademicYearExistsSpecification(Guid teacherId, int academicYearId)
    {
        Query.Where(x => x.TeacherId == teacherId && x.AcademicYearId == academicYearId);
    }
}