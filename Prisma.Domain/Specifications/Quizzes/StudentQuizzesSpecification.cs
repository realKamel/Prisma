using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class StudentQuizzesSpecification:Specification<Quiz>
{

    public StudentQuizzesSpecification(List<int?> enrolledLessonIds, int? academicYearId, Guid studentId)
    {
        Query
            .Where(q =>
                (q.Scope == QuizScope.LessonQuiz
                    && q.LessonId != null
                    && enrolledLessonIds.Contains(q.LessonId))
                || (q.Scope == QuizScope.ComprehensiveExam
                    && q.AcademicYearId == academicYearId))
            .Include(q => q.Questions)
            .Include(q => q.Attempts.Where(a => a.StudentId == studentId));
    }

}
