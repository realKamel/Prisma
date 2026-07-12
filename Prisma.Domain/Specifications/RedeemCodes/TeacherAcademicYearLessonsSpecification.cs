using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

// Returns AcademicYearLesson rows for academic years the given teacher is linked to.
public class TeacherAcademicYearLessonsSpecification : Specification<AcademicYearLesson>
{
    public TeacherAcademicYearLessonsSpecification(Guid teacherId)
    {
        Query.Where(x => x.AcademicYear.Teachers.Any(t => t.TeacherId == teacherId))
            .Include(x => x.Lesson);
    }
}