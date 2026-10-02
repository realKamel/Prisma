using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.AssignmentSubmissions.Queries.GetAssignmentSubmission;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.AssignmentSubmissions.Queries.GetAssignmentSubmission;

public class GetAssignmentSubmissionQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IStorageService storageService
) : IRequestHandler<GetAssignmentSubmissionQuery, Result<AssignmentSubmissionDto?>>
{
    public async Task<Result<AssignmentSubmissionDto?>> Handle(
        GetAssignmentSubmissionQuery request,
        CancellationToken cancellationToken
    )
    {
        var studentId = currentUserService.UserId;
        if (studentId is null)
            return Result.Unauthorized();

        var assignmentRepo = unitOfWork.GetOrCreateRepository<Assignment, int>();
        var assignment = await assignmentRepo.FirstOrDefaultAsync(
            new AssignmentWithEnrollmentSpec(request.LessonId),
            cancellationToken
        );

        if (assignment is null)
            return Result.NotFound($"No assignment found for lesson '{request.LessonId}'");

        var isEnrolled = assignment.Lesson.Enrollments.Any(e => e.StudentId == studentId);
        if (!isEnrolled)
            return Result.Unauthorized();

        var submission = assignment.Submissions.FirstOrDefault(s => s.StudentId == studentId);
        if (submission is null)
            return Result.Success<AssignmentSubmissionDto?>(null);

        // Submission rows are only ever created once the file upload succeeds,
        // so if the row exists the object is guaranteed to be in storage.
        var downloadUrl = await storageService.GetDownloadUrlAsync(
            storageService.DefaultBucketName,
            submission.FileUrl!
        );

        return Result.Success<AssignmentSubmissionDto?>(
            new AssignmentSubmissionDto
            {
                Title = submission.Title ?? string.Empty,
                FileUrl = downloadUrl,
                SubmittedAt = submission.SubmittedAt,
                Score = submission.Score,
                Notes = submission.Notes,
            }
        );
    }
}