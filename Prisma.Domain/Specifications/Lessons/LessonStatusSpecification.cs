using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonStatusSpecification : Specification<Lesson, LessonStatusProjection>
{
    public LessonStatusSpecification(int lessonId, Guid studentId)
    {
        Query.Where(lesson => lesson.Id == lessonId).AsNoTracking().
        Select(lesson => new LessonStatusProjection
        {
            Id = lesson.Id,

            HasEnrollment = lesson.Enrollments.Any(e => e.StudentId == studentId),

            EnrollmentExpiresAt = lesson.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.ExpiresAt)
                .FirstOrDefault(),

            HasPrerequisite = lesson.Prerequisite != null,

            IsPrerequisiteCompleted = lesson.Prerequisite != null &&
                lesson.Prerequisite.Enrollments
                    .Where(e => e.StudentId == studentId)
                    .Select(e => (bool?)e.IsCompleted)
                    .FirstOrDefault() == true
        });

    
    }
}
public class LessonStatusProjection
{
    public int Id { get; set; }
    public DateTimeOffset? EnrollmentExpiresAt { get; set; }
    public bool HasEnrollment { get; set; }
    public bool HasPrerequisite { get; set; }
    public bool IsPrerequisiteCompleted { get; set; }
}