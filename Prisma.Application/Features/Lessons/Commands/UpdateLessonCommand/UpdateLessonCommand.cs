using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.UpdateLessonDetails;

public record UpdateLessonDetailsCommand(
    int Id,
    string Title,
    string? Description,
    decimal Price,
    int? ValidityDays,
    int? PrerequisiteLessonId,
    List<ChapterCommandDto> Chapters,
    bool AssignmentEnabled,
  //  string? AssignmentDescription,
    DateTimeOffset? AssignmentDueDate,
    string? AssignmentFileTypes
) : IRequest<Result<string>>;

public record ChapterCommandDto(string Name, string? VideoFileName);