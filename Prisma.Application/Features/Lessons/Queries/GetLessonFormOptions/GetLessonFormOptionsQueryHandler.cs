using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonFormOptions;

public class GetLessonFormOptionsQueryHandler(
    IUnitOfWork unitOfWork, IIdentityService identityService, ICurrentUserService currentUserService
) : IRequestHandler<GetLessonFormOptionsQuery, Result<LessonFormOptionsResponseDto>>
{
    public async Task<Result<LessonFormOptionsResponseDto>> Handle(
        GetLessonFormOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized("User is not authenticated.");

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
            return Result.NotFound("User not found.");

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }
        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson,int>();
        var allLessons = await lessonRepository.ListAsync(
            new LessonWithProjectionSpec<LessonDto>(userId.Value,
            l => new LessonDto(l.Title ?? string.Empty, l.Id)),cancellationToken);
        

        var academicYearRepository = unitOfWork.GetOrCreateRepository<AcademicYear, int>();
        var allAcademicYears = await academicYearRepository.ListAsync(cancellationToken);
        var allAcademicYearsOptions = allAcademicYears
            .Select(ay => new AcademicYearResponseDto(ay.Id, ay.Title ?? string.Empty))
            .ToList();

        return Result<LessonFormOptionsResponseDto>.Success(
            new LessonFormOptionsResponseDto(allLessons, allAcademicYearsOptions)
        );


    }
}

