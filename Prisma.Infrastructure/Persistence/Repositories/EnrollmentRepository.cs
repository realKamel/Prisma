using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.ReadModels.Enrollments;
using Prisma.Domain.Repositories;

namespace Prisma.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository : Repository<Enrollment, int>, IEnrollmentRepository
{
    private readonly AppDbContext _dbContext;

    public EnrollmentRepository(AppDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StudentPerformanceReadModel?> GetPerformanceStatsAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .Set<Enrollment>()
            .Where(e => e.StudentId == userId)
            .GroupBy(e => e.StudentId)
            .Select(g => new StudentPerformanceReadModel(
                // 1. Count distinct lessons
                g.Select(e => e.LessonId).Distinct().Count(),
                // 2. Count completed lessons
                g.Count(e => e.IsCompleted),
                // 3. Sum study hours (convert TimeSpan to seconds for safe SQL translation)
                (int)(g.Sum(e => e.Lesson != null ? e.Lesson.Duration.TotalSeconds : 0.0) / 3600.0),
                // 4. Average quiz degree (null-safe to avoid skewing results)
                g.Average(e =>
                    e.Lesson != null && e.Lesson.Quiz != null
                        ? e.Lesson.Quiz.TotalDegree
                        : (decimal?)null
                ) ?? 0m
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
