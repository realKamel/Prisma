using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications;

public sealed class BookmarkByStudentSectionAndPositionSpecification : Specification<Bookmark>
{
    public BookmarkByStudentSectionAndPositionSpecification(
        Guid studentId,
        int sectionId,
        int positionSeconds
    )
    {
        Query.Where(b =>
            b.StudentId == studentId
            && b.SectionId == sectionId
            && b.PositionSeconds == positionSeconds
        );
    }
}