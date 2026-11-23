using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications;

public sealed class BookmarksByStudentAndSectionSpecification : Specification<Bookmark>
{
    public BookmarksByStudentAndSectionSpecification(Guid studentId, int sectionId)
    {
        Query.Where(b => b.StudentId == studentId && b.SectionId == sectionId);
    }
}