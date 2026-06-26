using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetTeacherLessonsForGrant;

public class GetTeacherLessonsForGrantQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTeacherLessonsForGrantQuery, List<LessonForGrantDto>>
{
    public async Task<List<LessonForGrantDto>> Handle(GetTeacherLessonsForGrantQuery request, CancellationToken cancellationToken)
    {
        var lessonRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.LessonAggregate.Lesson, int>();
        var lessons = await lessonRepo.ListAsync(new TeacherLessonsSpec(Guid.Empty), cancellationToken);

        return lessons.Select(l => new LessonForGrantDto(
            l.Id,
            l.Title ?? "—",
            $"{l.Sections?.Count ?? 0}")).ToList();
    }
}