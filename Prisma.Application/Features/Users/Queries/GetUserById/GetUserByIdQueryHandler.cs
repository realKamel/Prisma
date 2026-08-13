using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Ardalis.Result;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUserByIdQuery, Result<UserEditDto>>
{
    public async Task<Result<UserEditDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return Result.NotFound($"User with id '{request.Id}' was not found");

        var role = user switch
        {
            Student => AppRoles.Student,
            Teacher => AppRoles.Teacher,
            Assistant => AppRoles.Assistant,
            Domain.Entities.UserAggregate.Admin => AppRoles.Admin,
            _ => "Unknown",
        };

        // TeacherId is only meaningful for Student — Assistant→Teacher isn't
        // modeled in the DB (see AssistantConfiguration), so it stays null there.
        var dto = new UserEditDto(
            user.Id,
            user.FirstName,
            user.SecondName,
            user.ThirdName,
            user.LastName,
            user.PhoneNumber,
            user.Email,
            role,
            (user as Student)?.AcademicYearId,
            (user as Student)?.TeacherStudents.Select(ts => ts.TeacherId).ToList(),
            (user as Student)?.ParentPhoneNumber);

        return Result<UserEditDto>.Success(dto);
    }
}