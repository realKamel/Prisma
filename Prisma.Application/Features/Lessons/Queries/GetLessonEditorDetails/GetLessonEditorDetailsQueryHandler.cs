using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;

public class GetLessonEditorDetailsQueryHandler(
    IUnitOfWork _unitOfWork,
    IStorageService storageService,
    ICurrentUserService currentUserService,
    IIdentityService identityService)
    : IRequestHandler<GetLessonEditorDetailsQuery, Result<LessonEditorResponseDto>>
{
    public async Task<Result<LessonEditorResponseDto>> Handle(GetLessonEditorDetailsQuery request,
        CancellationToken cancellationToken)
    {

        var userId = currentUserService.UserId;
        if (userId is null)
            return Result.Unauthorized("User is not authenticated.");

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
            return Result.NotFound("User not found.");

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }

        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var academicYearRepository = _unitOfWork.GetOrCreateRepository<AcademicYear, int>();

        var spec = new GetLessonEditorDetailsSpecification(request.Id);
        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
            return Result.NotFound($"Lesson with id '{request.Id}' was not found");

        var prerequisiteSpec = new LessonPrerequisiteOptionsSpecification(request.Id, userId.Value);
        var prerequisitesOptions = (await lessonRepository.ListAsync(prerequisiteSpec, cancellationToken))
            .Select(l => new LessonDto(l.Title ?? string.Empty, l.Id))
            .ToList();

        var allAcademicYears = await academicYearRepository.ListAsync(cancellationToken);
        var allAcademicYearsOptions = allAcademicYears
            .Select(ay => new AcademicYearResponseDto(ay.Id, ay.Title ?? string.Empty))
            .ToList();

        var thumbnail = lesson.ImageThumbnailUrl != null
            ? await storageService.GetDownloadUrlAsync(storageService.DefaultBucketName, lesson.ImageThumbnailUrl)
            : string.Empty;

        var response = new LessonEditorResponseDto(
            lesson.Id,
            lesson.Title,
            lesson.Description,
            lesson.Price,
            lesson.PrerequisiteId,
            lesson.Sections.OrderBy(s => s.SortOrder).Select(s => new ChapterResponseDto(s.Title, s.ContentURL)).ToList(),
            lesson.HasAssignment,
            lesson.AssignmentDueDate,
            lesson.AssignmentTitle,
            thumbnail,
            lesson.Outcomes,
            lesson.AcademicYearIds,
            prerequisitesOptions,
            allAcademicYearsOptions
        );

        return Result<LessonEditorResponseDto>.Success(response);
    }
}