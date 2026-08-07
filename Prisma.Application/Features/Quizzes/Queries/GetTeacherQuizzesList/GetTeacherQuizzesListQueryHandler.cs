
using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Specifications;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizzesList;

public class GetTeacherQuizzesListQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeacherQuizzesListQuery, Result<TeacherQuizzesListResponseDto>>
{
    public async Task<Result<TeacherQuizzesListResponseDto>> Handle(GetTeacherQuizzesListQuery request, CancellationToken ct)
    {
        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();

        var quizzes = await quizRepo.ListAsync(
                new TeacherQuizzesListSpecification(request.Scope, request.Search), ct);

        var items = quizzes.Select(q =>
        {

            double? averageScore = null;

            if (q.AverageDegree.HasValue && q.TotalDegree > 0)
            {
                averageScore = Math.Round(
                    (double)(q.AverageDegree.Value / q.TotalDegree * 100),
                    1);
            }

            // Compute status:
            // - "completed"      => all attempts are Graded (and at least one exists)
            // - "pending_grading"=> at least one attempt is Submitted (needs manual grading)
            // - "active"         => no Submitted attempts (either no attempts yet, or all Graded/InProgress)

            string status;
            if (q.PendingGradingCount > 0)
                status = "pending_grading";
            else if (q.HasAttempts && !q.HasUngradedAttempts)
                status = "completed";
            else
                status = "active";

            return new TeacherQuizListItemDto
            {
                QuizId = q.QuizId,
                Title = q.Title ?? string.Empty,
                Description = q.Description,
                DurationMinutes = (int)q.TimeInMinutes.TotalMinutes,
                QuestionsCount = q.QuestionsCount,
                TotalDegree = q.TotalDegree,
                AvailableFrom = q.AvailableFrom,
                DueDate = q.DueDate,
                SubmittedCount = q.SubmittedCount,
                PendingGradingCount = q.PendingGradingCount,
                AverageScore = averageScore,
                Status = status
            };
        }).ToList();

        // Apply status filter in-memory (computed field, can't filter in DB)
        if (!string.IsNullOrWhiteSpace(request.Status))
            items = items.Where(i => i.Status == request.Status).ToList();
        var totalCount = items.Count;

        // Pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var pagedItems = items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new TeacherQuizzesListResponseDto
        {
            Items = pagedItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
