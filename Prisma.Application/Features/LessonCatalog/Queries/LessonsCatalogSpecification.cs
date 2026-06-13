using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Specifications;

namespace Prisma.Application.Features.LessonCatalog.Queries;

public class LessonsCatalogSpecification : BaseSpecification<Lesson>
{
    public LessonsCatalogSpecification()
    {
        AddInclude(x => x.Enrollments);
        AddInclude(x => x.Sections);
        AddInclude($"{nameof(Lesson.Sections)}.{nameof(Section.Progresses)}");
    }
}