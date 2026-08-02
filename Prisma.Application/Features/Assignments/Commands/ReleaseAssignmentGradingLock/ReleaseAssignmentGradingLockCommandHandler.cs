using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assignments;

namespace Prisma.Application.Features.Assignments.Commands.ReleaseAssignmentGradingLock;

public class ReleaseAssignmentGradingLockCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser)
    : IRequestHandler<ReleaseAssignmentGradingLockCommand, Result>
{
    public async Task<Result> Handle(
        ReleaseAssignmentGradingLockCommand request, CancellationToken ct)
    {
        var submissionRepo = unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>();
        var submission = await submissionRepo.FirstOrDefaultAsync(
            new SubmissionByIdSpecification(request.SubmissionId), ct);

        if (submission is null)
            return Result.Error("التسليم غير موجود");

        // Not locked — nothing to do, treat as success (idempotent)
        if (!submission.IsBeingGraded)
            return Result.SuccessWithMessage("القفل غير موجود بالفعل");

        // Only the user who holds the lock can release it
        if (submission.GradingByUserId != currentUser.UserId)
            return Result.Error("مينفعش تفكي قفل تصحيح شخص تاني");

        submission.IsBeingGraded = false;
        submission.GradingStartedAt = null;
        submission.GradingByUserId = null;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.SuccessWithMessage("تم إلغاء قفل التصحيح");
    }
}
