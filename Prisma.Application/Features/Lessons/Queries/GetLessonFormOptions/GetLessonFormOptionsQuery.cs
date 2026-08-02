using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;

public record GetLessonFormOptionsQuery : IRequest<Result<LessonFormOptionsResponseDto>>;

public record LessonFormOptionsResponseDto(
    List<LessonDto> PrerequisitesOptions,
    List<AcademicYearResponseDto> AllAcademicYearsOptions
);