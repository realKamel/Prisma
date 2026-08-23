using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

public class TeacherCodeBatchesSpecification<TResult> : Specification<RedeemCode, TResult>
{
    public TeacherCodeBatchesSpecification(Guid teacherId,
        Expression<Func<RedeemCode, TResult>> projection,
        int? academicYearId,
        int? lessonId)
    {
        if (academicYearId.HasValue)
            Query.Where(b => b.AcademicYearId == academicYearId.Value);

        if (lessonId.HasValue)
            Query.Where(b => b.LessonId == lessonId.Value);

        Query.Where(b => b.Lesson != null && b.Lesson.TeacherId == teacherId )
            .Include(b => b.Lesson)
            .Include(b => b.AcademicYear)
            .Include(b => b.GeneratedCodes)
            .OrderByDescending(b => b.CreatedAt)
            .AsNoTracking()
            .Select(projection);
    }
}
