using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetGradingList;

public class GetGradingListQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetGradingListQuery, Result<GradingListResponseDto>>
{
    public async Task<Result<GradingListResponseDto>> Handle(GetGradingListQuery request, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var attempts = await attemptRepo.ListAsync(
            new GradingAttemptsSpecification(request.Scope, request.QuizId), ct);

        // Apply search in-memory (student name or quiz title)
        var filtered = attempts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            filtered = filtered.Where(a =>
                $"{a.Student.FirstName} {a.Student.LastName}"
                    .Contains(search, StringComparison.OrdinalIgnoreCase)
                || (a.Quiz.Title ?? string.Empty)
                    .Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "all")
        {
            filtered = request.Status switch
            {
                "submitted" => filtered.Where(a => a.Status == QuizAttemptStatus.Submitted),
                "graded" => filtered.Where(a => a.Status == QuizAttemptStatus.Graded),
                _ => filtered
            };
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        // Pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = filteredList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new GradingListItemDto
            {
                AttemptId = a.Id,
                StudentId = a.StudentId,
                StudentName = $"{a.Student.FirstName} {a.Student.SecondName} {a.Student.ThirdName} {a.Student.LastName}".Trim(),
                QuizId = a.QuizId,
                QuizTitle = a.Quiz.Title ?? string.Empty,
                SubmittedAt = a.SubmittedAt,
                Status = a.Status == QuizAttemptStatus.Submitted ? "submitted" : "graded",
                Score = a.Status == QuizAttemptStatus.Graded ? a.Degree : null,
                PenaltyScore = a.PenaltyScore,
                TotalDegree = a.Quiz.TotalDegree,

                // Count answers that still need grading (written with no score yet)
                PendingWrittenCount = a.Answers.Count(ans => ans.Score == null),

                // Show 0 if no attempt exists
                TabSwitchCount = a?.TabSwitchCount ?? 0,
                CopyPasteAttemptCount = a?.CopyPasteAttemptCount ?? 0,

                HeldForSecurityReview = a?.Status == QuizAttemptStatus.Submitted
                && a.Answers.All(ans => ans.Score != null) 
                && (a.TabSwitchCount + a.CopyPasteAttemptCount) > 0 
            })
            .ToList();

        return new GradingListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
