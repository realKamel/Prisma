using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentByStudentSpecification : Specification<Enrollment>
{
    public EnrollmentByStudentSpecification(Guid studentId)
    {
        Query
            .Where(e => e.StudentId == studentId)
            .AsNoTrackingWithIdentityResolution();
    }

    public EnrollmentByStudentSpecification(Guid enrollmentId, bool isTracking)
    {
        Query.Where(e => e.PublicId == enrollmentId);

        if (!isTracking)
        {
            Query.AsNoTrackingWithIdentityResolution();
        }
    }
}