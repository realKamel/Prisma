using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Assignments.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assignments;

namespace Prisma.Application.Features.Assignments.Queries.GetAssignmentSubmissionDetail;

internal class GetAssignmentSubmissionDetailQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<GetAssignmentSubmissionDetailQuery, Result<AssignmentSubmissionDetailDto>>
{
    public async Task<Result<AssignmentSubmissionDetailDto>> Handle(GetAssignmentSubmissionDetailQuery request, CancellationToken ct)
    {
        var submissionRepo = unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>();
        AssignmentSubmission? submission = await submissionRepo.FirstOrDefaultAsync(
            new SubmissionDetailSpecification(request.SubmissionId), ct);

        if (submission is null)
            return Result<AssignmentSubmissionDetailDto>.Failure("التسليم غير موجود");

        // Acquire grading lock
        var now = DateTimeOffset.UtcNow;
        var lockExpired = submission.GradingStartedAt.HasValue
            && now - submission.GradingStartedAt.Value > TimeSpan.FromMinutes(30);

        if (submission.IsBeingGraded && !lockExpired)
        {
            return Result<AssignmentSubmissionDetailDto>.Failure(
                "التسليم ده بيتصحح دلوقتي من شخص تاني");
        }

        // Set grading lock
        submission.IsBeingGraded = true;
        submission.GradingStartedAt = now;
        submission.GradingByUserId = currentUser.UserId;
        await unitOfWork.SaveChangesAsync(ct);

        return new AssignmentSubmissionDetailDto
        {
            SubmissionId = submission.Id,
            StudentName = $"{submission.Student.FirstName} {submission.Student.LastName}".Trim(),
            LessonTitle = submission.Assignment.Lesson?.Title ?? string.Empty,
            SubmittedAt = submission.SubmittedAt,
            DueDate = submission.Assignment.DueDate,
            IsLateSubmission = submission.SubmittedAt > submission.Assignment.DueDate,
            FileUrl = submission.FileUrl,
            MaxScore = submission.Assignment.Grade,
            CurrentScore = submission.Score,
            CurrentNote = submission.Notes
        };

    }
}
