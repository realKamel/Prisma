using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonPlayerWithDetailsSpecification : Specification<Lesson>
{
    public LessonPlayerWithDetailsSpecification(int lessonId)
        : base()
    {
        Query.Where(lesson => lesson.Id == lessonId)
            .Include(l => l.Sections)
            .ThenInclude(s => s.Progresses)
            .Include(l => l.Quiz)
            .ThenInclude(q => q.Questions)
            .Include(l => l.Assignment)
            .Include(l => l.LessonMaterials)
            .Include(l => l.Enrollments)
            .AsNoTracking();


    }
}