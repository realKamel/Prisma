using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Teachers;

public class TeacherLessonWithDetailsSpecification : Specification<Lesson>
{
    public TeacherLessonWithDetailsSpecification(int lessonId)
        : base()
    {
        Query.Where(lesson => lesson.Id == lessonId && !lesson.IsDeleted)
            .Include(l => l.Sections)     
            .Include(l => l.Assignment);  

    }
}