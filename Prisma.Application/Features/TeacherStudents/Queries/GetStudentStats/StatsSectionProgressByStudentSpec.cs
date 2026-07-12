using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentStats;

public class StatsSectionProgressByStudentSpec : Specification<SectionProgress>
{
    public StatsSectionProgressByStudentSpec(Guid studentId)
    {
        Query.Where(sp => sp.StudentId == studentId);
    }
}
