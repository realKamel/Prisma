using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.ReadModels.Enrollments;

namespace Prisma.Domain.Repositories;

public interface IEnrollmentRepository : IRepository<Enrollment, int>
{
    Task<StudentPerformanceReadModel?> GetPerformanceStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
