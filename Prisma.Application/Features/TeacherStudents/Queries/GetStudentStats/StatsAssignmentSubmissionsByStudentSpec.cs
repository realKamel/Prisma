using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentStats;

public class StatsAssignmentSubmissionsByStudentSpec : Specification<AssignmentSubmission>
{
    public StatsAssignmentSubmissionsByStudentSpec(Guid studentId)
    {
        Query.Where(s => s.StudentId == studentId);
    }
}
