using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Prisma.Domain.ValueObjects.ContentDomain;
using Prisma.Domain.ValueObjects.EnrollmentDomain;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonDetailsCommand;

public record CreateLessonDetailsCommand(
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
    DateTimeOffset ValidityUnTill
) : IRequest<Result<CreateLessonResponse>>;

public record ChapterCreateDto(string Name, string? VideoFileName, double VideoDurationSeconds);

public record CreateLessonResponse(int lessonId, List<int> sectionIds);
