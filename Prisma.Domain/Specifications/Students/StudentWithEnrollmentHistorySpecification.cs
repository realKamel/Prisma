using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public class StudentWithEnrollmentHistorySpecification : Specification<Student>
{
    public StudentWithEnrollmentHistorySpecification(string email)
    {
        Query
            .Where(s => s.Email == email)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Lesson)
            .ThenInclude(l => l.Sections)
            .ThenInclude(s => s.Progresses)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Lesson)
            .ThenInclude(l => l.Quiz)
            .AsNoTracking();
    }
}