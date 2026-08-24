using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Users.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.TeacherStudent;

namespace Prisma.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IIdentityService identityService)
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
        var teacherStudentsRepo = unitOfWork.GetOrCreateRepository<TeacherStudent, int>();
        await teacherStudentsRepo.ListAsync(new TeacherByStudentSpec(user.Id), cancellationToken);
        List<string> teacherIds = user is Student ?
            (user as Student).TeacherStudents.Select(ts => ts.TeacherId.ToString()).ToList() : new();

        if (user is Assistant) teacherIds.Add((user as Assistant).TeacherId.Value.ToString());

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
            teacherIds,
            (user as Student)?.ParentPhoneNumber);

        return Result<UserEditDto>.Success(dto);
    }
}