using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Users.Commands.CreateUser;

internal sealed class CreateUserCommandHandler(IIdentityService identityService, IUnitOfWork uow)
    : IRequestHandler<CreateUserCommand, Result<UserEditDto>>
{
    public async Task<Result<UserEditDto>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken
    )
    {
        var existing = await identityService.FindByEmailOrPhoneAsync(
            request.Email,
            request.Mobile,
            cancellationToken
        );

        if (existing is not null)
        {
            return Result.Conflict("A user with this email or phone already exists.");
        }

        var roleInRequest = request.Role.ToLowerInvariant();

        User user;
        switch (request.Role)
        {
            case AppRoles.Student:
                user = Student.Create(
                    request.FirstName,
                    request.SecondName,
                    request.Email,
                    request.Mobile,
                    request.GradeId,
                    request.ParentMobile,
                    request.ThirdName,
                    request.LastName
                );
                break;
            case AppRoles.Teacher:
                user = Teacher.Create(
                    request.FirstName,
                    request.SecondName,
                    request.Email,
                    request.Mobile,
                    request.Subject,
                    request.ThirdName,
                    request.LastName
                );
                break;
            case AppRoles.Assistant:
                user = Assistant.Create(
                    request.FirstName,
                    request.SecondName,
                    request.Email,
                    request.Mobile,
                    request.TeacherId,
                    request.ThirdName,
                    request.LastName
                );
                break;
            case AppRoles.Admin:
                user = User.Create(
                    request.FirstName,
                    request.SecondName,
                    request.Email,
                    request.Mobile,
                    request.ThirdName,
                    request.LastName
                );
                break;
            default:
                return Result.Error($"Unknown role '{request.Role}'.");
        }

        var createResult = await identityService.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return Result.Error(string.Join("\n", createResult.Errors.Select(e => e.Code)));
        }

        var roleResult = await identityService.AddToRoleAsync(user, request.Role);

        if (!roleResult.Succeeded)
        {
            return Result.Error(string.Join("\n", roleResult.Errors.Select(e => e.Description)));
        }

        if (roleInRequest == AppRoles.Teacher)
        {
            var academicYearsTeachers = uow.GetOrCreateRepository<AcademicYear, int>();

            var academicYears = await academicYearsTeachers.ListAsync(cancellationToken);

            foreach (var ay in academicYears)
            {
                ay.RegisterTeacherInAcademicYear(user.Id);
            }
        }

        await uow.SaveChangesAsync(cancellationToken);
        var dto = new UserEditDto(
            user.Id,
            user.FirstName,
            user.SecondName,
            user.ThirdName,
            user.LastName,
            user.PhoneNumber,
            user.Email,
            request.Role,
            (user as Student)?.AcademicYearId,
            [],
            (user as Student)?.ParentPhoneNumber
        );

        return dto;
    }
}
