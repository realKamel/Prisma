using MediatR;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.UpdateLessonDetails;

public record UpdateLessonDetailsCommand(
    int Id,
    string Title,
    string? Description,
    decimal Price,
    int? PrerequisiteLessonId,
    List<ChapterCommandDto> Chapters,
    bool AssignmentEnabled,
    IFormFile? AssignmentFile,
    DateTimeOffset? AssignmentDueDate,
    bool IsPublished,
    List<int> AcademicYearIds ,
    List<string> Outcomes,
    IFormFile? ImageFile
) : IRequest<Result<UpdateLessonResponse>>;
public record UpdateLessonResponse(List<NewSectionResult> NewSections);
public record NewSectionResult(int SectionId, int ChapterIndex);

public record ChapterCommandDto(string Name, string? VideoFileName);