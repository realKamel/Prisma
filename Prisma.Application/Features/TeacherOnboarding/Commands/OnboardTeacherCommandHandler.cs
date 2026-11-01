using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AcademicYearsSpecs;

namespace Prisma.Application.Features.TeacherOnboarding.Commands;

internal sealed class OnboardTeacherCommandHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService
) : IRequestHandler<OnboardTeacherCommand, Result>
{
    public async Task<Result> Handle(
        OnboardTeacherCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await identityService.FindByEmailOrPhoneAsync(
            request.Email,
            request.PhoneNumber,
            cancellationToken
        );

        if (user is not null)
        {
            return Result.Conflict("TEACHER.ALREADY_EXISTS");
        }

        var teacherCreateResult = Teacher.Create(
            request.FirstName,
            request.SecondName,
            request.Email,
            request.PhoneNumber,
            request.ThirdName,
            request.LastName
        );

        if (!teacherCreateResult.IsSuccess)
        {
            return teacherCreateResult.Map();
        }

        var teacher = teacherCreateResult.Value;

        var identityResult = await identityService.CreateAsync(teacher, request.Password);

        if (!identityResult.Succeeded)
        {
            return Result.Error(
                string.Join("\n", identityResult.Errors.Select(e => e.Description))
            );
        }

        var roleResult = await identityService.AddToRoleAsync(teacher, AppRoles.Teacher);

        if (!roleResult.Succeeded)
        {
            return Result.Error(string.Join("\n", roleResult.Errors.Select(e => e.Description)));
        }

        var academicYearsTeachers = unitOfWork.GetOrCreateRepository<AcademicYear, int>();

        var academicYears = await academicYearsTeachers.ListAsync(
            new FilteredAcademicYear(request.AcademicYears),
            cancellationToken
        );

        foreach (var ay in academicYears)
        {
            ay.RegisterTeacherInAcademicYear(teacher.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        //this should be created
        return Result.Success();
    }
}
