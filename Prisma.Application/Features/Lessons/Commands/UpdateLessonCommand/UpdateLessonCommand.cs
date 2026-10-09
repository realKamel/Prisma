using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Prisma.Application.Features.Lessons.Commands.UpdateLessonCommand;

public record UpdateLessonDetailsCommand(
    int Id,
    string Title,
    string? Description,
    decimal Price,
    int? PrerequisiteLessonId,
    IList<ChapterCommandDto> Chapters,
    bool AssignmentEnabled,
    IFormFile? AssignmentFile,
    DateTimeOffset? AssignmentDueDate,
    bool IsPublished,
    IList<int> AcademicYearIds,
    IList<string> Outcomes,
    IFormFile? ImageFile,
    string Currency = "EGP"
) : IRequest<Result<UpdateLessonResponse>>;

public record UpdateLessonResponse(IList<NewSectionResult> NewSections);

public record NewSectionResult(int SectionId, int ChapterIndex);

public record ChapterCommandDto(string Name, string? VideoFileName, double VideoDurationSeconds);
