using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assignments;

namespace Prisma.Application.Features.Assignments.Commands.GradeAssignmentSubmission;

internal class GradeAssignmentSubmissionCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<GradeAssignmentSubmissionCommand, Result>
{
    public async Task<Result> Handle(GradeAssignmentSubmissionCommand request, CancellationToken ct)
    {
        var submissionRepo = unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>();
        var submission = await submissionRepo.FirstOrDefaultAsync(
            new SubmissionWithAssignmentSpecification(request.SubmissionId), ct);

        if (submission is null)
            return Result.Failure("التسليم غير موجود");

        // Validate score doesn't exceed max
        if (request.Score > submission.Assignment.Grade)
            return Result.Failure(
                $"الدرجة ({request.Score}) أكبر من الدرجة الكاملة ({submission.Assignment.Grade})");

        // Validate grading lock — only the user who opened the modal can grade
        var now = DateTimeOffset.UtcNow;
        var lockExpired = submission.GradingStartedAt.HasValue
            && now - submission.GradingStartedAt.Value > TimeSpan.FromMinutes(30);

        if (submission.IsBeingGraded
            && !lockExpired
            && submission.GradingByUserId != currentUser.UserId)
        {
            return Result.Failure("التسليم ده بيتصحح دلوقتي من شخص تاني");
        }

        // Apply grading
        submission.Score = request.Score;
        submission.Notes = request.Note;

        // Release grading lock
        submission.IsBeingGraded = false;
        submission.GradingStartedAt = null;
        submission.GradingByUserId = null;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success("تم حفظ التصحيح بنجاح");

    }
}
