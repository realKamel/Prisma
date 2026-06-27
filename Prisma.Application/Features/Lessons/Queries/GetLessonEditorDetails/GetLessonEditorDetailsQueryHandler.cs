using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;

public class GetLessonEditorDetailsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetLessonEditorDetailsQuery, Result<LessonEditorResponseDto>>
{
    public async Task<Result<LessonEditorResponseDto>> Handle(GetLessonEditorDetailsQuery request, CancellationToken cancellationToken)
    {
        var lessonRepository = _unitOfWork.GetOrCreateRepository<Lesson, int>();
        var academicYearRepository = _unitOfWork.GetOrCreateRepository<AcademicYear, int>();

        var spec = new GetLessonEditorDetailsSpecification(request.Id);
        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
            throw new NotFoundException("Lesson", request.Id);

        var allLessons = await lessonRepository.ListAsync(cancellationToken);
        var prerequisitesOptions = allLessons
            .Where(l => l.Id != request.Id && !l.IsDeleted)
            .Select(l => new LessonDto(l.Title ?? string.Empty, l.Id))
            .ToList();

        var allAcademicYears = await academicYearRepository.ListAsync(cancellationToken);
        var allAcademicYearsOptions = allAcademicYears
            .Select(ay => new AcademicYearResponseDto(ay.Id, ay.Title ?? string.Empty))
            .ToList();
        var existingAcademicYearIds = lesson.AcademicYears?
            .Select(ay => ay.AcademicYearId)
            .ToList() ?? new List<int>();

        var response = new LessonEditorResponseDto(
            lesson.Id,
            lesson.Title,
            lesson.Description,
            lesson.Price,
            lesson.PrerequisiteId,
            lesson.Sections.OrderBy(s => s.SortOrder).Select(s => new ChapterResponseDto(s.Title, s.ContentURL)).ToList(),
            lesson.Assignment != null,
            lesson.Assignment?.DueDate,
            lesson.Assignment?.ContentURL,
            lesson.ImageThumbnailUrl,
            lesson.Outcomes?.ToList() ?? new List<string>(),
            existingAcademicYearIds,
            
            prerequisitesOptions,

                allAcademicYearsOptions
            );

        return Result<LessonEditorResponseDto>.Success(response);
    }
}