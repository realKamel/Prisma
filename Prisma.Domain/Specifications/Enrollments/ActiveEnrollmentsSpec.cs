using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Enrollments;

public sealed class ActiveEnrollmentsSpec : Specification<Enrollment>
{
    public ActiveEnrollmentsSpec(Guid teacherId, DateTimeOffset? before = null)
    {
        Query
            .Where(e =>
                e.Status == EnrollmentStatus.Active
                && e.Lesson!.TeacherId == teacherId
                && (before == null || e.CreatedAt <= before)
            )
            .AsNoTracking();
    }

    public ActiveEnrollmentsSpec(
        Guid studentId,
        int pageIndex,
        int pageSize,
        DateTimeOffset? before = null
    )
    {
        Query
            .Where(e =>
                e.Status == EnrollmentStatus.Active
                && e.StudentId == studentId
                && (before == null || e.CreatedAt <= before)
            )
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Lesson)
            .ThenInclude(l => l.Teacher)
            .Include(x => x.Lesson.Sections)
            .ThenInclude(p => p.Progresses)
            .AsSplitQuery()
            .AsNoTrackingWithIdentityResolution();
    }
}
