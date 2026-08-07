using Ardalis.Specification;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Application.Features.Quizzes.Specifications;

public class QuizWithAttemptsSpecification
    : Specification<Quiz, QuizStudentsProjection>
{
    public QuizWithAttemptsSpecification(int quizId)
    {
        Query
            .Where(q => q.Id == quizId)

            .Select(q => new QuizStudentsProjection
            {
                Id = q.Id,
                Title = q.Title,
                TotalDegree = q.TotalDegree,
                Scope = q.Scope,
                LessonId = q.LessonId,
                AcademicYearId = q.AcademicYearId,
                DueDate = q.DueDate,

                Attempts = q.Attempts.Select(a => new QuizAttemptProjection
                {
                    StudentId = a.StudentId,
                    Status = a.Status,
                    Degree = a.Degree,
                    SubmittedAt = a.SubmittedAt,
                    TabSwitchCount = a.TabSwitchCount,
                    CopyPasteAttemptCount = a.CopyPasteAttemptCount,

                    PendingWrittenCount =
                        a.Answers.Count(ans => ans.Score == null)
                }).ToList()
            });
    }
}