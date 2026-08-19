using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.SubmitAssignmentCommand;

public class SubmitAssignmentCommandHandler(
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
        var assignmentRepo = unitOfWork.GetOrCreateRepository<Assignment, int>();

        var assignment = await assignmentRepo.FirstOrDefaultAsync(
            new AssignmentWithEnrollmentSpec(request.LessonId),
            cancellationToken
        );

        if (assignment is null)
            return Result.Error("لا يوجد واجب لهذا الدرس");

        var studentId = currentUser.UserId;

        if (studentId is null)
            return Result.Error("سجل دخولك اولا");

        var isEnrolled = assignment.Lesson.Enrollments.Any(e => e.StudentId == studentId);

        if (!isEnrolled)
            return Result.Error("غير مصرح لك بتسليم هذا الواجب");

        var alreadySubmitted = assignment.Submissions.Any(s => s.StudentId == studentId);

        if (alreadySubmitted)
            return Result.Error("لقد سلمت هذا الواجب مسبقاً");

        var objectKey =
            $"assignments/{assignment.Id}/students/{studentId}/{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";

        await using var stream = request.File.OpenReadStream();

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
