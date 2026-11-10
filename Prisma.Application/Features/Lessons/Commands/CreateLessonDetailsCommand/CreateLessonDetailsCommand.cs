using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetailsCommand;

public sealed record CreateLessonDetailsCommand(
    string Title,
    string? Description,
    decimal Price,
    string Currency,
    int? PrerequisiteLessonId,
    IList<ChapterCreateDto> Chapters,
    bool AssignmentEnabled,
    IFormFile? AssignmentFile,
    DateTimeOffset? AssignmentDueDate,
    bool IsPublished,
    IList<int> AcademicYearIds,
    IList<string> Outcomes,
    IFormFile? ImageFile,
    Guid? TeacherId,
    long ValidityDurationInSeconds
) : ICommand<Result<CreateLessonResponse>>;

public record ChapterCreateDto(string Name, string? VideoFileName, double VideoDurationSeconds);

public record CreateLessonResponse(int lessonId, List<int> sectionIds);
