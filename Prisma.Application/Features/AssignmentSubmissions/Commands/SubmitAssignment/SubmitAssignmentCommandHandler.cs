using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.AssignmentSubmissions.Commands.SubmitAssignment;

internal sealed class SubmitAssignmentCommandHandler(
    IUnitOfWork unitOfWork,
    IStorageService storage,
    ICurrentUserService currentUser
) : IRequestHandler<SubmitAssignmentCommand, Result>
{
    public async Task<Result> Handle(
        SubmitAssignmentCommand request,
        CancellationToken cancellationToken
    )
    {
        var studentId = currentUser.UserId;

        if (studentId is null)
        {
            return Result.Unauthorized("سجل دخولك اولا");
        }

        var assignmentRepo = unitOfWork.GetOrCreateRepository<Assignment, int>();

        var assignment = await assignmentRepo.FirstOrDefaultAsync(
            new AssignmentWithEnrollmentSpec(request.LessonId),
            cancellationToken
        );

        if (assignment is null)
        {
            return Result.NotFound("لا يوجد واجب لهذا الدرس");
        }

        var isEnrolled = assignment.Lesson.Enrollments.Any(e => e.StudentId == studentId);

        if (!isEnrolled)
        {
            return Result.Error("غير مصرح لك بتسليم هذا الواجب");
        }

        var alreadySubmitted = assignment.Submissions.Any(s => s.StudentId == studentId);

        if (alreadySubmitted)
        {
            return Result.Conflict("لقد سلمت هذا الواجب مسبقاً");
        }

        var objectKey =
            $"assignments/{assignment.Id}/students/{studentId}/{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";

        await using var stream = request.File.OpenReadStream();

        //TODO: this must be fixed to make client upload directly

        await storage.UploadFileAsync(
            bucketName: storage.DefaultBucketName,
            objectKey: objectKey,
            content: stream,
            contentType: request.File.ContentType,
            cancellationToken: cancellationToken
        );

        var submission = new AssignmentSubmission
        {
            StudentId = studentId.Value,
            AssignmentId = assignment.Id,
            FileUrl = objectKey,
            Title = request.File.FileName,
            SubmittedAt = DateTimeOffset.UtcNow,
        };

        assignment.Submissions.Add(submission);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
