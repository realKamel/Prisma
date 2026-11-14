using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

// Returns AcademicYearLesson rows for academic years the given teacher is linked to.
public sealed class TeacherAcademicYearLessonsSpecification<TResult>
    : Specification<AcademicYearLesson, TResult>
{
    public TeacherAcademicYearLessonsSpecification(
        Guid teacherId,
        Expression<Func<AcademicYearLesson, TResult>> projection
    )
    {
        Query
            .Where(x =>
                x.AcademicYear.Teachers.Any(t => t.TeacherId == teacherId)
                && x.Lesson.TeacherId == teacherId
            )
            .Select(projection);
    }
}
