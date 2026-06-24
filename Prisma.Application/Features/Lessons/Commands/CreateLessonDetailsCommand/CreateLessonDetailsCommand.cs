using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;

public record CreateLessonDetailsCommand(
    string Title,
    string? Description,
    decimal Price,
    int? PrerequisiteLessonId,
    List<ChapterCreateDto> Chapters,
    bool AssignmentEnabled,
    DateTimeOffset? AssignmentDueDate,
    string? AssignmentFileTypes,
    bool IsPublished
) : IRequest<Result<int>>; 

public record ChapterCreateDto(string Name, string? VideoFileName);