using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentByStudentSpecification : Specification<Enrollment>
{
    public EnrollmentByStudentSpecification(Guid studentId)
    {
        Query.AsNoTrackingWithIdentityResolution().Where(e => e.StudentId == studentId);
    }
}
