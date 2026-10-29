using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Sections;

public sealed class SectionByPublicLessonId : Specification<Section>
{
    public SectionByPublicLessonId(Guid publicSectionId, Guid publicLessonId, bool tracking = false)
    {
        Query.Where(l => l.PublicId == publicSectionId && l.Lesson.PublicId == publicLessonId);

        if (!tracking)
        {
            Query.AsNoTracking();
        }
    }
}
