using Ardalis.Specification;

namespace Prisma.Domain.Specifications.RedeemCodes;

public sealed class TeacherAcademicYearExistsSpecification
    : Specification<Prisma.Domain.Entities.LessonAggregate.AcademicYearTeacher>
{
    public TeacherAcademicYearExistsSpecification(Guid teacherId, int academicYearId)
    {
        Query.Where(x => x.TeacherId == teacherId && x.AcademicYearId == academicYearId);
    }
}
