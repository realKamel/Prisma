using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentActivities;

public class SectionProgressByStudentSpec : Specification<SectionProgress>
{
    public SectionProgressByStudentSpec(Guid studentId)
    {
        Query.Where(sp => sp.StudentId == studentId)
             .Include(sp => sp.Section)
             .ThenInclude(s => s!.Lesson)
             .OrderByDescending(sp => sp.CreatedAt);
    }
}
