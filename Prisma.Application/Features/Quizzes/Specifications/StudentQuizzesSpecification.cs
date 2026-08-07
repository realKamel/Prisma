
using Ardalis.Specification;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Specifications;

public class StudentQuizzesSpecification : Specification<Quiz, StudentQuizzesListProjection>
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

            .Select(q => new StudentQuizzesListProjection
            {
                Id = q.Id,
                Title = q.Title,
                TotalDegree = q.TotalDegree,
                AvailableFrom = q.AvailableFrom,
                DueDate = q.DueDate,
                DurationMinutes = (int)q.TimeInMinutes.TotalMinutes,

                QuestionsCount = q.Questions.Count,

                Attempt = q.Attempts
                        .Where(a => a.StudentId == studentId)
                        .Select(a => new AttemptProjection
                        {
                            Id = a.Id,
                            Status = a.Status,
                            Degree = a.Degree,
                            SubmittedAt = a.SubmittedAt
                        })
                        .FirstOrDefault()
            });
    }

}