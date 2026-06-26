using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentStats;

public class StatsQuizAttemptsByStudentSpec : Specification<QuizAttempt>
{
    public StatsQuizAttemptsByStudentSpec(Guid studentId)
    {
        Query.Where(q => q.StudentId == studentId);
    }
}
