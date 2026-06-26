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
    bool IsPublished,
    List<string> Outcomes,
    string? ImageUrl,
    List<int> AcademicYearIds
) : IRequest<Result<int>>;

public record ChapterCreateDto(string Name, string? VideoFileName);