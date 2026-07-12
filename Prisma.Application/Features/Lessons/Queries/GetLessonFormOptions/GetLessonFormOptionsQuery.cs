using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;

public record GetLessonFormOptionsQuery : IRequest<Result<LessonFormOptionsResponseDto>>;

public record LessonFormOptionsResponseDto(
    List<LessonDto> PrerequisitesOptions,
    List<AcademicYearResponseDto> AllAcademicYearsOptions
);