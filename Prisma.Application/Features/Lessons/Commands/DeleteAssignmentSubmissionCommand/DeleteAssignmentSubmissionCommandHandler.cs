using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.DeleteAssignmentSubmissionCommand;

public class DeleteSubmissionCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IStorageService storage
) : IRequestHandler<DeleteSubmissionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSubmissionCommand request,
        CancellationToken cancellationToken
    )
    {
        var studentId = currentUser.UserId;
        if (studentId is null)
            return Result.Unauthorized("سجل دخولك اولا");

        var assignmentRepo = unitOfWork.GetOrCreateRepository<Assignment, int>();
        var assignment = await assignmentRepo.FirstOrDefaultAsync(
            new AssignmentWithEnrollmentSpec(request.LessonId),
            cancellationToken
        );

        if (assignment is null)
            return Result.NotFound(
                $"{nameof(Assignment)} with id '{request.LessonId}' was not found"
            );

        if (assignment.DueDate < DateTimeOffset.UtcNow)
            return Result.Error("انتهى الموعد النهائي للتسليم");

        var submission = assignment.Submissions.FirstOrDefault(s => s.StudentId == studentId);
        if (submission is null)
            return Result.NotFound(
                $"{nameof(AssignmentSubmission)} with id '{request.LessonId}' was not found"
            );

        try
        {
            await storage.DeleteFileAsync(
                storage.DefaultBucketName,
                submission.FileUrl,
                cancellationToken
            );
        }
        catch { }

        assignment.Submissions.Remove(submission);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessWithMessage("تم حذف التسليم بنجاح");
    }
}
