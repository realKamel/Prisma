using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonsCatalogSpecification : Specification<Lesson>
{
    public LessonsCatalogSpecification(int academicYearId)
    {
        Query
            .Where(x =>
                x.Status == LessonStatus.Active
                && x.AcademicYears.Any(ay => ay.AcademicYearId == academicYearId)
            )
            .Include(x => x.Enrollments)
            .Include(y => y.Teacher)
            .Include(x => x.Sections)
            .ThenInclude(s => s.Progresses)
            .AsSplitQuery();
    }
}
