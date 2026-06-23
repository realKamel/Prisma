using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizStudents;

public class GetQuizStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetQuizStudentsQuery, Result<QuizStudentsResponseDto>>
{
    public async Task<Result<QuizStudentsResponseDto>> Handle(GetQuizStudentsQuery request, CancellationToken ct)
    {
        // 1. Fetch quiz with all attempts and their answers
        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();
        var quiz = await quizRepo.FirstOrDefaultAsync(
            new QuizWithAttemptsSpecification(request.QuizId), ct);

        if (quiz is null)
            return Result<QuizStudentsResponseDto>.Failure("الاختبار غير موجود");

        // 2. Build attempt lookup by studentId
        var attemptByStudent = quiz.Attempts.ToDictionary(a => a.StudentId);

        // 3. Get all enrolled students based on scope
        List<Student> enrolledStudents;

        if (quiz.Scope == QuizScope.LessonQuiz && quiz.LessonId.HasValue)
        {
            var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
            var enrollments = await enrollmentRepo.ListAsync(
                new EnrolledStudentsByLessonSpecification(quiz.LessonId.Value), ct);
            enrolledStudents = enrollments
                .Where(e => e.Student != null)
                .Select(e => e.Student!)
                .ToList();
        }
        else
        {
            // ComprehensiveExam — get all students in the academic year
            var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
            var students = await studentRepo.ListAsync(
                new StudentsByAcademicYearSpecification(quiz.AcademicYearId!.Value), ct);
            enrolledStudents = students;
        }

        var now = DateTimeOffset.UtcNow;

        // 4. Map each student to their attempt status
        var allItems = enrolledStudents.Select(student =>
        {
            attemptByStudent.TryGetValue(student.Id, out var attempt);

            string status;
            decimal? score = null;
            DateTimeOffset? submittedAt = null;
            int pendingWrittenCount = 0;

            if (attempt is null)
            {
                // No attempt — check if quiz deadline has passed
                status = quiz.DueDate.HasValue && quiz.DueDate < now
                    ? "missed"
                    : "not_started";
            }
            else
            {
                submittedAt = attempt.SubmittedAt;

                switch (attempt.Status)
                {
                    case QuizAttemptStatus.InProgress:
                        status = "in_progress";
                        break;

                    case QuizAttemptStatus.Submitted:
                        status = "submitted";
                        // Count written answers that still need grading
                        pendingWrittenCount = attempt.Answers
                            .Count(a => a.Score == null);
                        break;

                    case QuizAttemptStatus.Graded:
                        status = "graded";
                        score = attempt.Degree;
                        break;

                    default:
                        status = "not_started";
                        break;
                }
            }

            return new QuizStudentAttemptDto
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                AttemptStatus = status,
                SubmittedAt = submittedAt,
                Score = score,
                TotalDegree = quiz.TotalDegree,
                PendingWrittenCount = pendingWrittenCount
            };
        }).ToList();

        // 5. Apply search filter (by student name)
        if (!string.IsNullOrWhiteSpace(request.Search))
            allItems = allItems
                .Where(s => s.StudentName.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        // 6. Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status))
            allItems = allItems
                .Where(s => s.AttemptStatus == request.Status)
                .ToList();

        // 7. Summary counts (before pagination, after filters)
        var totalCount = allItems.Count;
        var submittedCount = allItems.Count(s =>
            s.AttemptStatus == "submitted" || s.AttemptStatus == "graded");
        var pendingGradingCount = allItems.Count(s => s.AttemptStatus == "submitted");
        var gradedCount = allItems.Count(s => s.AttemptStatus == "graded");

        // 8. Pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var pagedItems = allItems
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new QuizStudentsResponseDto
        {
            QuizId = quiz.Id,
            Title = quiz.Title ?? string.Empty,
            TotalDegree = quiz.TotalDegree,
            TotalStudents = enrolledStudents.Count,
            SubmittedCount = submittedCount,
            PendingGradingCount = pendingGradingCount,
            GradedCount = gradedCount,
            Students = pagedItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
