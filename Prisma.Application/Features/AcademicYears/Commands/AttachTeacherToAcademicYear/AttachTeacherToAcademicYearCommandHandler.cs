using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AcademicYearsSpecs;

namespace Prisma.Application.Features.AcademicYears.Commands.AttachTeacherToAcademicYear;

public sealed class AttachTeacherToAcademicYearCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : ICommandHandler<AttachTeacherToAcademicYearCommand, Result>
{
    public async Task<Result> Handle(
        AttachTeacherToAcademicYearCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        Guid teacherId;

        switch (currentUserService.Role)
        {
            case AppRoles.Teacher:
                teacherId = userId.Value;
                break;

            case AppRoles.Admin:
                teacherId = request.TeacherId.Value;
                break;

            default:
                return Result.Unauthorized();
        }

        var academicYearRepo = unitOfWork.GetOrCreateRepository<AcademicYear, int>();

        var academicYear = await academicYearRepo.FirstOrDefaultAsync(
            new AcademicYearByIdSpec(request.AcademicYearId),
            cancellationToken
        );

        if (academicYear is null)
        {
            return Result.NotFound();
        }

        var registrationResult = academicYear.RegisterTeacher(teacherId);

        if (!registrationResult.IsSuccess)
        {
            return registrationResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
