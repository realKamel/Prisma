using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Students.Commands.ChangePasswordCommand;


public class ChangePasswordCommandHandler(
    UserManager<User> _userManager,
    ICurrentUserService _currentUserService
) : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result.Unauthorized("User is not authenticated.");

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            return Result.NotFound($"User with id '{userId.Value}' was not found");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<bool>.Error($"فشلت عملية تغيير كلمة المرور: {errorMessages}");
        }

        return Result<bool>.Success(true);
    }
}