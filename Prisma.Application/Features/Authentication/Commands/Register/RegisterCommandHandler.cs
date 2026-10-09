using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.UserAggregate;
using Ardalis.Result;

namespace Prisma.Application.Features.Authentication.Commands.Register;

internal sealed class RegisterCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, Result>
{
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser =
            await identityService.FindByEmailOrPhoneAsync(request.Email, request.PhoneNumber, cancellationToken);

        if (existingUser is not null)
        {
            return Result.Invalid();
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
                .Select(e =>
                    new ValidationError(e.Key, string.Join(",", e.Select(y => y.Description))));
            return Result.Invalid(errors);
        }

        await identityService.AddToRoleAsync(user, AppRoles.Student);

        return Result.NoContent();
    }
}