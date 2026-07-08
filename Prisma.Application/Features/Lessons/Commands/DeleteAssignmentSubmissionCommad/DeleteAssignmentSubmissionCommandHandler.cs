using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Commands.DeleteAssignmentSubmissionCommand;
public class DeleteSubmissionCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IStorageService storage) : IRequestHandler<DeleteSubmissionCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteSubmissionCommand request, CancellationToken cancellationToken)
    {
        var studentId = currentUser.UserId;
        if (studentId is null)
            throw new UnauthorizedException("سجل دخولك اولا");

        var assignmentRepo = unitOfWork.GetOrCreateRepository<Assignment, int>();
        var assignment = await assignmentRepo.FirstOrDefaultAsync(
            new AssignmentWithEnrollmentSpec(request.LessonId), cancellationToken);

        if (assignment is null)
            throw new NotFoundException(nameof(Assignment), request.LessonId);

        if (assignment.DueDate < DateTimeOffset.UtcNow)
            throw new BadRequestException("انتهى الموعد النهائي للتسليم");

        var submission = assignment.Submissions.FirstOrDefault(s => s.StudentId == studentId);
        if (submission is null)
            throw new NotFoundException(nameof(AssignmentSubmission), request.LessonId);

        try
        {
            await storage.DeleteFileAsync("prisma", submission.FileUrl, cancellationToken);
        }
        catch { }

        assignment.Submissions.Remove(submission);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("تم حذف التسليم بنجاح");
    }
}