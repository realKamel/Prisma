using Ardalis.Specification;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Students;

public sealed class StudentHasReportSinceSpec : Specification<Student>
{
    public StudentHasReportSinceSpec(Guid studentId, DateTimeOffset since)
    {
        Query
            .Where(s => s.Id == studentId)
            .Where(s => s.Reports.Any(r => r.Date > since));
    }
}