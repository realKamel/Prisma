using MediatR;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Common.Responses.Generic;

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
) : IRequest<Result<int>>;

public record ChapterCreateDto(string Name, string? VideoFileName);