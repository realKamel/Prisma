using Ardalis.Specification;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Domain.Specifications.RedeemCodes;

public class TeacherCodeBatchesSpecification : Specification<RedeemCode>
{
    public TeacherCodeBatchesSpecification(
        Guid teacherId,
        int? academicYearId,
        int? lessonId)
    {
        Query.Where(b => b.CreatedByTeacherId == teacherId);

        if (academicYearId.HasValue)
            Query.Where(b => b.AcademicYearId == academicYearId.Value);

        if (lessonId.HasValue)
            Query.Where(b => b.LessonId == lessonId.Value);

        Query.Include(b => b.Lesson)
            .Include(b => b.AcademicYear)
            .Include(b => b.GeneratedCodes)
            .OrderByDescending(b => b.CreatedAt);
    }
}