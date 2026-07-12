using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Commands.RevokeLesson;

public class RevokeLessonCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<RevokeLessonCommand, Result>
{
    public async Task<Result> Handle(RevokeLessonCommand request, CancellationToken cancellationToken)
    {
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();

        // IgnoreQueryFilters in spec so we can find even previously soft-deleted rows
        var enrollment = await enrollmentRepo.FirstOrDefaultAsync(
            new EnrollmentByStudentAndLessonSpec(request.StudentId, request.LessonId),
            cancellationToken);

        if (enrollment is null)
            throw new NotFoundException("Enrollment", $"{request.StudentId}-{request.LessonId}");

        // Soft delete — row stays in DB, access is revoked
        enrollment.IsDeleted = true;
        enrollment.DeletedAt = DateTimeOffset.UtcNow;
        enrollment.DeletedBy = currentUserService.UserId;

        enrollmentRepo.Update(enrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Lesson access revoked successfully.");
    }
}