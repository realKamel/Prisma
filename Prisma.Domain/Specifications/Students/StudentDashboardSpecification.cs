using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public class StudentDashboardSpecification : Specification<Student>
{
    public StudentDashboardSpecification(Guid userId)
    {
        Query
            .Where(s => s.Id == userId)
            .Include(s => s.AcademicYear)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Lesson)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Lesson)
                .ThenInclude(l => l.Sections)
                .ThenInclude(s => s.Progresses)
            .Include(s => s.QuizAttempts)
            .AsSplitQuery()                        
            .AsNoTracking();
    }
}