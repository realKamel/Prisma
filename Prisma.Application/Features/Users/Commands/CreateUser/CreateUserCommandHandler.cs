using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<CreateUserCommand, Result<UserEditDto>>
{
    public async Task<Result<UserEditDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await identityService.FindByEmailOrPhoneAsync(request.Email, request.Mobile, cancellationToken);
        if (existing is not null)
            return Result.Conflict("A user with this email or phone already exists.");

        if (request.Role is not (AppRoles.Student or AppRoles.Teacher or AppRoles.Assistant or AppRoles.Admin))
            return Result.Error($"Unknown role '{request.Role}'.");

        User user = request.Role switch
        {
            AppRoles.Student => new Student
            {
                Id = Guid.CreateVersion7(),
                AcademicYearId = request.GradeId,
                ParentPhoneNumber = request.ParentMobile,
                TeacherId = request.TeacherId,
                CreatedAt = DateTimeOffset.UtcNow,
            },
            AppRoles.Teacher => new Teacher { Id = Guid.CreateVersion7(), CreatedAt = DateTimeOffset.UtcNow },
            // NOTE: unlike CreateAssistantCommand, this generic path doesn't set
            // Policies claims — an admin can grant permissions afterward via the
            // existing AssistantsController.UpdateAssistantPermissions endpoint.
            AppRoles.Assistant => new Assistant { Id = Guid.CreateVersion7(), CreatedAt = DateTimeOffset.UtcNow },
            AppRoles.Admin => new Domain.Entities.UserAggregate.Admin { Id = Guid.CreateVersion7(), CreatedAt = DateTimeOffset.UtcNow },
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
            return Result.Error(string.Join("\n", createResult.Errors.Select(e => e.Description)));

        var roleResult = await identityService.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
            return Result.Error(string.Join("\n", roleResult.Errors.Select(e => e.Description)));

        var dto = new UserEditDto(
            user.Id, user.FirstName, user.SecondName, user.ThirdName, user.LastName,
            user.PhoneNumber, user.Email, request.Role,
            (user as Student)?.AcademicYearId,
            (user as Student)?.TeacherId,
            (user as Student)?.ParentPhoneNumber);

        return Result<UserEditDto>.Success(dto, "User created successfully.");
    }
}