using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        var spec = new GetLessonEditorDetailsSpecification(request.Id);

        var lesson = await lessonRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (lesson is null)
            throw new NotFoundException("Lesson", request.Id);

        // عمل Mapping يدوي ونظيف للداتا لتطابق الفرونت إند بالملي 🚀
        var response = new LessonEditorResponseDto(
            lesson.Id,
            lesson.Title,
            lesson.Description,
            lesson.Price,
            lesson.PrerequisiteId,
            lesson.Sections.OrderBy(s => s.SortOrder).Select(s => new ChapterResponseDto(s.Title, s.ContentURL)).ToList(),
            lesson.Assignment != null,
            lesson.Assignment?.DueDate,
            lesson.Assignment?.ContentURL
        );

        return Result<LessonEditorResponseDto>.Success(response);
    }
}