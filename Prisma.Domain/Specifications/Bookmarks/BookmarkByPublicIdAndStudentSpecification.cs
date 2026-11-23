using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications;

public sealed class BookmarkByPublicIdAndStudentSpecification : Specification<Bookmark>
{
    public BookmarkByPublicIdAndStudentSpecification(Guid publicId, Guid studentId)
    {
        Query.Where(b => b.PublicId == publicId && b.StudentId == studentId);
    }
}