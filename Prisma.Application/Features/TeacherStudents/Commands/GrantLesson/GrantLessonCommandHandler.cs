using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Commands.GrantLesson;

public class GrantLessonCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<GrantLessonCommand, Result>
{
    public async Task<Result> Handle(GrantLessonCommand request, CancellationToken cancellationToken)
    {
        var studentRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.UserAggregate.Student, Guid>();
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var lessonRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.LessonAggregate.Lesson, int>();

        var student = await studentRepo.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
            throw new NotFoundException("Student", request.StudentId);

        var lesson = await lessonRepo.GetByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson", request.LessonId);

        var existingEnrollments = await enrollmentRepo.ListAsync(
            new EnrollmentByStudentAndLessonSpec(request.StudentId, request.LessonId), cancellationToken);
        if (existingEnrollments.Any())
            throw new BadRequestException("Student is already enrolled in this lesson.");

        var enrollment = new Enrollment
        {
            Status = EnrollmentStatus.Active,
            EnrollmentMethod = EnrollmentMethod.TeacherGrant,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(request.ValidityDays),
            LessonId = request.LessonId,
            StudentId = request.StudentId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await enrollmentRepo.AddAsync(enrollment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Lesson granted successfully.");
    }
}