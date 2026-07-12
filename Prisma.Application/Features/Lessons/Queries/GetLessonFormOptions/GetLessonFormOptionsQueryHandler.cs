using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonFormOptions;

public class GetLessonFormOptionsQueryHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<GetLessonFormOptionsQuery, Result<LessonFormOptionsResponseDto>>
{
    public async Task<Result<LessonFormOptionsResponseDto>> Handle(
        GetLessonFormOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var lessonRepository = unitOfWork.GetOrCreateRepository<Lesson,int>();
        var allLessons = await lessonRepository.ListAsync(cancellationToken);
        var prerequisitesOptions = allLessons
            .Select(l => new LessonDto(l.Title ?? string.Empty, l.Id))
            .ToList();

        var academicYearRepository = unitOfWork.GetOrCreateRepository<AcademicYear, int>();
        var allAcademicYears = await academicYearRepository.ListAsync(cancellationToken);
        var allAcademicYearsOptions = allAcademicYears
            .Select(ay => new AcademicYearResponseDto(ay.Id, ay.Title ?? string.Empty))
            .ToList();

        return Result<LessonFormOptionsResponseDto>.Success(
            new LessonFormOptionsResponseDto(prerequisitesOptions, allAcademicYearsOptions)
        );
    }
}
