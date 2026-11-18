using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonStatusSpecification : Specification<Lesson, LessonStatusProjection>
{
    public LessonStatusSpecification(Guid lessonId, Guid studentId)
    {
        Query
            .Where(lesson => lesson.PublicId == lessonId)
            .AsNoTracking()
            .Select(lesson => new LessonStatusProjection
            {
                PublicId = lesson.PublicId,
                HasEnrollment = lesson.Enrollments.Any(e => e.StudentId == studentId),
                EnrollmentExpiresAt = lesson
                    .Enrollments.Where(e => e.StudentId == studentId)
                    .Select(e => e.ExpiresAt)
                    .FirstOrDefault(),
                HasPrerequisite = lesson.Prerequisite != null,
                HasPrerequisiteEnrollment =
                    lesson.Prerequisite != null
                    && lesson.Prerequisite.Enrollments.Any(e => e.StudentId == studentId),
                IsPrerequisiteCompleted =
                    lesson.Prerequisite != null
                    && lesson.Prerequisite.Enrollments.Any(e =>
                        e.StudentId == studentId && e.IsCompleted
                    ),
            });
    }
}

public class LessonStatusProjection
{
    public Guid PublicId { get; set; }
    public DateTimeOffset? EnrollmentExpiresAt { get; set; }
    public bool HasEnrollment { get; set; }
    public bool HasPrerequisite { get; set; }
    public bool HasPrerequisiteEnrollment { get; set; }
    public bool IsPrerequisiteCompleted { get; set; }
}