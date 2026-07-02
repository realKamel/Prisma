using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Teachers;

public class GetLessonEditorDetailsSpecification : Specification<Lesson>
{
    public GetLessonEditorDetailsSpecification(int lessonId)
        : base()
    {
        Query.Where(lesson => lesson.Id == lessonId)
             .Include(l => l.Sections)
             .Include(l => l.Assignment)
             .Include(l=>l.AcademicYears)

             .AsNoTracking(); 
    }
}