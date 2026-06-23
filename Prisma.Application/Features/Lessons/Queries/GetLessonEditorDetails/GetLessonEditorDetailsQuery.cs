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
    string? AssignmentFileTypes
);

public record ChapterResponseDto(string Name, string? VideoFileName);