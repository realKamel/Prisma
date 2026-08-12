using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonFormOptions;

public record GetLessonFormOptionsQuery : IRequest<Result<LessonFormOptionsResponseDto>>;

public record LessonFormOptionsResponseDto(
    List<LessonDto> PrerequisitesOptions,
    List<AcademicYearResponseDto> AllAcademicYearsOptions
);