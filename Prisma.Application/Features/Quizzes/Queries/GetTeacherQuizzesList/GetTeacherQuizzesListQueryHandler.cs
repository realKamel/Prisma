
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizzesList;

public class GetTeacherQuizzesListQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeacherQuizzesListQuery, Result<List<TeacherQuizListItemDto>>>
{
    public async Task<Result<List<TeacherQuizListItemDto>>> Handle(GetTeacherQuizzesListQuery request, CancellationToken ct)
    {
        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();

        var quizzes = await quizRepo.
            ListAsync(new TeacherQuizzesSpecification(request.Scope, request.Search), ct);

        var items = quizzes.Select(q =>
        {
            var gradedAttempts = q.Attempts
                .Where(a => a.Status == QuizAttemptStatus.Graded)
                .ToList();

            var pendingGradingCount = q.Attempts
                .Count(a => a.Status == QuizAttemptStatus.Submitted);

            var submittedCount = q.Attempts
                .Count(a => a.Status == QuizAttemptStatus.Submitted
                         || a.Status == QuizAttemptStatus.Graded);

            double? averageScore = gradedAttempts.Count > 0 && q.TotalDegree > 0
                ? Math.Round(gradedAttempts.Average(a => (double)(a.Degree / q.TotalDegree * 100)), 1)
                : null;

            // Compute status:
            // - "completed"      => all attempts are Graded (and at least one exists)
            // - "pending_grading"=> at least one attempt is Submitted (needs manual grading)
            // - "active"         => no Submitted attempts (either no attempts yet, or all Graded/InProgress)
            string status;
            if (pendingGradingCount > 0)
                status = "pending_grading";
            else if (q.Attempts.Any() && q.Attempts.All(a => a.Status == QuizAttemptStatus.Graded))
                status = "completed";
            else
                status = "active";

            return new TeacherQuizListItemDto
            {
                QuizId = q.Id,
                Title = q.Title ?? string.Empty,
                Description = q.Description,
                DurationMinutes = (int)q.TimeInMinutes.TotalMinutes,
                QuestionsCount = q.Questions.Count,
                TotalDegree = q.TotalDegree,
                AvailableFrom = q.AvailableFrom,
                DueDate = q.DueDate,
                SubmittedCount = submittedCount,
                PendingGradingCount = pendingGradingCount,
                AverageScore = averageScore,
                Status = status
            };
        }).ToList();

        // Apply status filter in-memory (computed field, can't filter in DB)
        if (!string.IsNullOrWhiteSpace(request.Status))
            items = items.Where(i => i.Status == request.Status).ToList();

        return items;
    }
}
