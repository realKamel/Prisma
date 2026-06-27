using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;

public record GetLessonEditorDetailsQuery(int Id) : IRequest<Result<LessonEditorResponseDto>>;

public record LessonEditorResponseDto(
    int Id,
    string? Title,
    string? Description,
    decimal Price,
    int? PrerequisiteLessonId,
    List<ChapterResponseDto> Chapters,
    bool AssignmentEnabled,
    DateTimeOffset? AssignmentDueDate,
    string? AssignmentFileTypes,
    string? ImageUrl,
    List<string>? Outcomes,
    List<int> SelectedAcademicYears,   
    List<LessonDto> PrerequisitesOptions,
    List<AcademicYearResponseDto> AllAcademicYearsOptions  
);

public record LessonDto(string Name, int Id);
public record ChapterResponseDto(string Name, string? VideoFileName);
public record AcademicYearResponseDto(int Id, string Name);