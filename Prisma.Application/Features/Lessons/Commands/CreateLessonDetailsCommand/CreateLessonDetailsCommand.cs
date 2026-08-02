using MediatR;
using Microsoft.AspNetCore.Http;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetails;

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
    List<int> AcademicYearIds ,
    List<string> Outcomes,
    IFormFile? ImageFile
) : IRequest<Result<CreateLessonResponse>>;

public record ChapterCreateDto(string Name, string? VideoFileName);

public record CreateLessonResponse(int lessonId, List<int> sectionIds);