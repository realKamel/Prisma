using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.AcademicYears.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AcademicYearTeacher;

namespace Prisma.Application.Features.AcademicYearTeacher.Queries.GetAllAcademicYearTeacher;

public class GetAllAcademicYearTeacherQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService)
    : IRequestHandler<GetAllAcademicYearTeacherQuery, Result<List<AcademicYearOptionDto>>>
{
    public async Task<Result<List<AcademicYearOptionDto>>> Handle(
        GetAllAcademicYearTeacherQuery request,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var user = await identityService.FindByIdAsync(userId.Value);

        if (user is null)
        {
            return Result.Unauthorized();
        }

        var roles = await identityService.GetRolesAsync(user);

        Guid teacherId=Guid.Empty;

        if (roles.Contains(AppRoles.Teacher))
        {
            teacherId = user.Id;
        }
        else if (roles.Contains(AppRoles.Assistant))
        {
            if (user is not Assistant assistant)
            {
                return Result.Error("Assistant record is missing teacher assignment.");
            }

            if (assistant.TeacherId is null)
            {
                return Result.Error("This assistant is not assigned to a teacher.");
            }

            teacherId = assistant.TeacherId.Value;
        }


        var repo = unitOfWork.GetOrCreateRepository<
            Prisma.Domain.Entities.LessonAggregate.AcademicYearTeacher, int>();

        var years = await repo.ListAsync(
            new AcademicYearTeacherWithProjection<AcademicYearOptionDto>(
                teacherId,
                a => new AcademicYearOptionDto
                {
                    Id = a.AcademicYearId,
                    Name = a.AcademicYear.Title ?? string.Empty
                }),
            ct);

        return years.ToList();
    }
}