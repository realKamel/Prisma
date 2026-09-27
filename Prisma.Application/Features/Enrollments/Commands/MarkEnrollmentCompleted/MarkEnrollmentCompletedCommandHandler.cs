using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;

namespace Prisma.Application.Features.Enrollments.Commands.MarkEnrollmentCompleted;

internal sealed class MarkEnrollmentCompletedCommandHandler(IUnitOfWork uow, ICurrentUserService currentUserService)
    : IRequestHandler<MarkEnrollmentCompletedCommand, Result>
{
    public async Task<Result> Handle(MarkEnrollmentCompletedCommand request, CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
        {
            return Result.Unauthorized();
        }

        var enrollmentRepo = uow.GetOrCreateRepository<Enrollment, int>();

        var enrollment =
            await enrollmentRepo.FirstOrDefaultAsync(
                new EnrollmentByStudentSpecification(request.EnrollmentId, true),
                cancellationToken);

        if (enrollment is null)
        {
            return Result.NotFound($"Enrollment with ID {request.EnrollmentId} not found.");
        }

        enrollment.CompleteEnrollment();

        await uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}