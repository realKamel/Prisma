using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentActivities;

public class QuizAttemptsByStudentSpec : Specification<QuizAttempt>
{
    public QuizAttemptsByStudentSpec(Guid studentId)
    {
        Query.Where(q => q.StudentId == studentId)
             .Include(q => q.Quiz)
             .OrderByDescending(q => q.CreatedAt);
    }
}
