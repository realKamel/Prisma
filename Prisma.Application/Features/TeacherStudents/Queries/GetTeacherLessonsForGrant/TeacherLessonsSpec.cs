using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetTeacherLessonsForGrant;

public class TeacherLessonsSpec : Specification<Lesson>
{
    public TeacherLessonsSpec(Guid teacherId)
    {
        Query.Include(l => l.Sections)
             .OrderBy(l => l.Title);
    }
}