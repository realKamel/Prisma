using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetailsCommand;

public record CreateLessonDetailsCommand(
    string Title,
    string? Description,
    decimal Price,
    int? PrerequisiteLessonId,
    List<ChapterCreateDto> Chapters,
    bool AssignmentEnabled,
    IFormFile? AssignmentFile,
    DateTimeOffset? AssignmentDueDate,
    bool IsPublished,
    List<int> AcademicYearIds,
    List<string> Outcomes,
    IFormFile? ImageFile,
    Guid? TeacherId
) : IRequest<Result<CreateLessonResponse>>;

public record ChapterCreateDto(string Name, string? VideoFileName);

public record CreateLessonResponse(int lessonId, List<int> sectionIds);