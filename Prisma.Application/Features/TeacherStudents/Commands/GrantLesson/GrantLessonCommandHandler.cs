using MediatR;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Common.Responses;
using Prisma.Application.Features.TeacherStudents;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Commands.GrantLesson;

public class GrantLessonCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<GrantLessonCommand, Result>
{
    public async Task<Result> Handle(GrantLessonCommand request, CancellationToken cancellationToken)
    {
        var studentRepo    = unitOfWork.GetOrCreateRepository<Domain.Entities.UserAggregate.Student, Guid>();
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var lessonRepo     = unitOfWork.GetOrCreateRepository<Domain.Entities.LessonAggregate.Lesson, int>();

        var student = await studentRepo.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
            throw new NotFoundException("Student", request.StudentId);

        var lesson = await lessonRepo.GetByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson", request.LessonId);

        // Find any enrollment including soft-deleted ones (IgnoreQueryFilters in spec)
        var existing = await enrollmentRepo.FirstOrDefaultAsync(
            new EnrollmentByStudentAndLessonSpec(request.StudentId, request.LessonId),
            cancellationToken);

        if (existing is not null)
        {
            if (!existing.IsDeleted)
                throw new BadRequestException("Student is already enrolled in this lesson.");

            // Student had this lesson before and was revoked — restore it
            existing.IsDeleted        = false;
            existing.DeletedAt        = null;
            existing.DeletedBy        = null;
            existing.Status           = EnrollmentStatus.Active;
            existing.EnrollmentMethod = EnrollmentMethod.TeacherGrant;
            existing.ExpiresAt        = DateTimeOffset.UtcNow.AddDays(request.ValidityDays);
            existing.UpdatedAt        = DateTimeOffset.UtcNow;

            // Force EF to treat this as a modification not an insertion
            unitOfWork.DbContext.Entry(existing).State = EntityState.Modified;
        }
        else
        {
            // Fresh enrollment — DB sequence generates the Id
            var enrollment = new Enrollment
            {
                Status           = EnrollmentStatus.Active,
                EnrollmentMethod = EnrollmentMethod.TeacherGrant,
                ExpiresAt        = DateTimeOffset.UtcNow.AddDays(request.ValidityDays),
                LessonId         = request.LessonId,
                StudentId        = request.StudentId,
                CreatedAt        = DateTimeOffset.UtcNow
            };

            enrollmentRepo.Add(enrollment);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Lesson granted successfully.");
    }
}