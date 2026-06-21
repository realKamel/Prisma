using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Exceptions;

namespace Prisma.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler(IIdentityService identityService) : IRequestHandler<RegisterCommand, Result>
{
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normailzedEmail = request.Email.ToUpper();

        // var existingUser = await userManager.Users
        //     .FirstOrDefaultAsync(u =>
        //             u.NormalizedEmail == normailzedEmail || u.PhoneNumber == request.PhoneNumber,
        //         cancellationToken);

        var existingUser = await identityService.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new BadRequestException("Registration Failed");
        }

        var user = new Student()
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            SecondName = request.SecondName,
            ThirdName = request.ThirdName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            AcademicYearId = request.AcademicYear,
            ParentPhoneNumber = request.ParentPhoneNumber
        };


        var result = await identityService.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(e => e.Description)
                        .ToArray()
                );

            throw new BadRequestException("Error happen. Try again later.");
        }

        await identityService.AddToRoleAsync(user, AppClaims.Roles.Student);

        return Result.Success("Registered successfully");
    }
}