using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
namespace Prisma.Domain.Specifications.Sections;

public class SectionProgressByStudentAndSectionSpecification : Specification<SectionProgress>
{
    public SectionProgressByStudentAndSectionSpecification( Guid studentId, int sectionId)
    {
        Query.Where(sp=>sp.SectionId==sectionId && sp.StudentId == studentId);
    }
}