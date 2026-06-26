using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Commands.RevokeLesson;

public class RevokeLessonCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RevokeLessonCommand, Result>
{
    public async Task<Result> Handle(RevokeLessonCommand request, CancellationToken cancellationToken)
    {
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();

        var enrollment = await enrollmentRepo.FirstOrDefaultAsync(
            new EnrollmentByStudentAndLessonSpec(request.StudentId, request.LessonId), cancellationToken);

        if (enrollment is null)
            throw new NotFoundException("Enrollment", $"{request.StudentId}-{request.LessonId}");

        await enrollmentRepo.DeleteAsync(enrollment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Lesson access revoked successfully.");
    }
}