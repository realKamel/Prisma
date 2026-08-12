using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(IIdentityService identityService, IUnitOfWork uow)
    : IRequestHandler<CreateUserCommand, Result<UserEditDto>>
{
    public async Task<Result<UserEditDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await identityService.FindByEmailOrPhoneAsync(request.Email, request.Mobile, cancellationToken);
        if (existing is not null)
            return Result.Conflict("A user with this email or phone already exists.");

        var roleInRequest = request.Role.ToLower();

        if (roleInRequest is not (AppRoles.Student or AppRoles.Teacher or AppRoles.Assistant or AppRoles.Admin))
            return Result.Error($"Unknown role '{request.Role}'.");

        User user = roleInRequest switch
        {
            AppRoles.Student => new Student
            {
                Id = Guid.CreateVersion7(),
                AcademicYearId = request.GradeId,
                ParentPhoneNumber = request.ParentMobile,
                TeacherId = request.TeacherId,
            },
            AppRoles.Teacher => new Teacher { Id = Guid.CreateVersion7() },
            // NOTE: unlike CreateAssistantCommand, this generic path doesn't set
            // Policies claims — an admin can grant permissions afterward via the
            // existing AssistantsController.UpdateAssistantPermissions endpoint.
            AppRoles.Assistant => new Assistant { Id = Guid.CreateVersion7() },
            AppRoles.Admin => new Domain.Entities.UserAggregate.Admin { Id = Guid.CreateVersion7() },
        };

        user.FirstName = request.FirstName;
        user.SecondName = request.SecondName;
        user.ThirdName = request.ThirdName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.Mobile;
        user.Email = request.Email;
        user.UserName = request.Email;

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
                ay.Teachers.Add(new() { TeacherId = user.Id, AcademicYearId = ay.Id });
            }
        }
        await uow.SaveChangesAsync(cancellationToken);
        var dto = new UserEditDto(
            user.Id, user.FirstName, user.SecondName, user.ThirdName, user.LastName,
            user.PhoneNumber, user.Email, request.Role,
            (user as Student)?.AcademicYearId,
            (user as Student)?.TeacherId,
            (user as Student)?.ParentPhoneNumber);

        return dto;
    }
}