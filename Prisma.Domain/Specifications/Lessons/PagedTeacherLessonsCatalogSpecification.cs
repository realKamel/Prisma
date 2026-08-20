using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Lessons;

public class PagedTeacherLessonsCatalogSpecification : Specification<Lesson>
{
    public PagedTeacherLessonsCatalogSpecification(
        Guid teacherId,
        string? keyword,
        int pageNumber,
        int pageSize,
        int academicYearId
    )
    {
        Query
            .Where(x => x.Status == LessonStatus.Active && x.TeacherId == teacherId)
            .Where(x => x.AcademicYears.Any(ay => ay.AcademicYearId == academicYearId))
            .Include(x => x.Enrollments)
            .Include(x => x.Sections)
            .ThenInclude(s => s.Progresses);

        if (keyword is not null)
        {
            Query.Search(x => x.Title, $"%{keyword}%");
        }

        Query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    public PagedTeacherLessonsCatalogSpecification(
        Guid teacherId,
        string? keyword,
        int academicYearId
    )
    {
        Query
            .Where(x => x.Status == LessonStatus.Active && x.TeacherId == teacherId)
            .Where(x => x.AcademicYears.Any(ay => ay.AcademicYearId == academicYearId))
            .Include(x => x.Enrollments)
            .Include(x => x.Sections)
            .ThenInclude(s => s.Progresses);

        if (keyword is not null)
        {
            Query.Search(x => x.Title, $"%{keyword}%");
        }
    }
}
