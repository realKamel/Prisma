using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Enrollments;

public sealed class PaginatedEnrollmentPerTeacherSpecification : Specification<Enrollment>
{
    public PaginatedEnrollmentPerTeacherSpecification(
        Guid teacherId,
        Guid studentId,
        string? keyword,
        int pageNumber,
        int pageSize
    )
    {
        Query
            .Where(x =>
                x.Lesson.Status == LessonStatus.Active
                && x.StudentId == studentId
                && x.Lesson.TeacherId == teacherId
            )
            .Include(x => x.Lesson)
            .ThenInclude(y => y.Teacher)
            .Include(x => x.Lesson)
            .ThenInclude(x => x.Sections)
            .ThenInclude(s => s.Progresses)
            .AsSplitQuery()
            .Skip(pageSize)
            .Take((pageNumber - 1) * pageSize)
            .AsNoTrackingWithIdentityResolution();

        if (keyword is not null)
        {
            Query.Search(x => x.Lesson.Title, $"%{keyword}%");
        }
    }
}
