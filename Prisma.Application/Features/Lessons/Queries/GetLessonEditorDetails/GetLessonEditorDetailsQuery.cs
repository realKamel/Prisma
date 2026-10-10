using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;

public record GetLessonEditorDetailsQuery(int Id) : IRequest<Result<LessonEditorResponseDto>>;

public record LessonEditorResponseDto(
    int Id,
    Guid PublicId,
    string? Title,
    string? Description,
    MoneyDto Money,
    int? PrerequisiteLessonId,
    IList<ChapterResponseDto> Chapters,
    bool AssignmentEnabled,
    DateTimeOffset? AssignmentDueDate,
    string? AssignmentFileName,
    string? ImageUrl,
    IList<string>? Outcomes,
    IList<int> SelectedAcademicYears,
    IList<LessonDto> PrerequisitesOptions,
    IList<AcademicYearResponseDto> AllAcademicYearsOptions
);

public record LessonDto(string Name, int Id);

public record ChapterResponseDto(string Name, string? VideoFileName);

public record AcademicYearResponseDto(int Id, string Name);
