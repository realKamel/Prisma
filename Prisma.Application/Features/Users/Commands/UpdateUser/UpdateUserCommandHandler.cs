using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler(
    IIdentityService identityService,
    UserManager<User> userManager)
    : IRequestHandler<UpdateUserCommand, Result<UserEditDto>>
{
    public async Task<Result<UserEditDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.NotFound($"User with id '{request.Id}' was not found");

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await identityService.FindByEmailAsync(request.Email);
            if (existing is not null && existing.Id != user.Id)
                return Result.Conflict("This email is already in use by another account.");

            user.Email = request.Email;
            user.UserName = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
            user.NormalizedUserName = request.Email.ToUpperInvariant();
        }

        user.FirstName = request.FirstName;
        user.SecondName = request.SecondName;
        user.ThirdName = request.ThirdName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.Mobile;

        if (user is Student student)
        {
            student.AcademicYearId = request.GradeId;
            student.ParentPhoneNumber = request.ParentMobile;
            student.TeacherId = request.TeacherId;
        }
        // Assistant→Teacher isn't modeled in the DB — TeacherId is ignored for
        // Assistant updates until that column/FK exists.

        var updateResult = await identityService.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Result.Error(string.Join("\n", updateResult.Errors.Select(e => e.Description)));

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var removeResult = await userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
                return Result.Error("Failed to reset password.");

            var addResult = await userManager.AddPasswordAsync(user, request.NewPassword);
            if (!addResult.Succeeded)
                return Result.Error(string.Join("\n", addResult.Errors.Select(e => e.Description)));
        }

        var role = user switch
        {
            Student => AppRoles.Student,
            Teacher => AppRoles.Teacher,
            Assistant => AppRoles.Assistant,
            Domain.Entities.UserAggregate.Admin => AppRoles.Admin,
            _ => "Unknown",
        };

        var dto = new UserEditDto(
            user.Id, user.FirstName, user.SecondName, user.ThirdName, user.LastName,
            user.PhoneNumber, user.Email, role,
            (user as Student)?.AcademicYearId,
            (user as Student)?.TeacherId,
            (user as Student)?.ParentPhoneNumber);

        return Result<UserEditDto>.Success(dto, "User updated successfully.");
    }
}