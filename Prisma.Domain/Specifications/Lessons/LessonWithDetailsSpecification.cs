using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.ValueObjects.ContentDomain;
using Prisma.Domain.ValueObjects.EnrollmentDomain;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonWithDetailsSpecification : Specification<Lesson, LessonDetailsProjection>
{
    public LessonWithDetailsSpecification(int lessonId)
    {
        Query
            .Where(lesson => lesson.Id == lessonId)
            .AsNoTracking()
            .Select(lesson => new LessonDetailsProjection
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Price = lesson.Price,
                ImageThumbnailUrl = lesson.ImageThumbnailUrl,
                EnrollmentsCount = lesson.Enrollments.Count,
                Money = lesson.Money,
                TimeDuration = lesson.TimeDuration,
                TeacherName = lesson.Teacher.FirstName + " " + lesson.Teacher.LastName,
                ValidityRange = lesson.ValidityRange,
                TeacherSubject = lesson.Teacher.Subject,
                Outcomes = lesson.Outcomes.ToList(),
                Sections = lesson
                    .Sections.Select(s => new SectionProjection
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Duration = s.Duration,
                        IsPreview = s.IsPreview,
                    })
                    .ToList(),
                PrerequisiteId = lesson.Prerequisite != null ? lesson.Prerequisite.Id : (int?)null,
                PrerequisiteTitle = lesson.Prerequisite != null ? lesson.Prerequisite.Title : null,
            });
    }
}

public record LessonDetailsProjection
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? ImageThumbnailUrl { get; init; }
    public int EnrollmentsCount { get; init; }
    public Money Money { get; init; }
    public TimeDuration TimeDuration { get; init; }
    public DateRange? ValidityRange { get; init; }
    public List<string> Outcomes { get; init; } = [];
    public List<SectionProjection> Sections { get; init; } = [];
    public int? PrerequisiteId { get; init; }
    public string? PrerequisiteTitle { get; init; }
    public string TeacherName { get; init; }
    public string TeacherSubject { get; init; }
}

public record SectionProjection
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public TimeSpan Duration { get; init; }
    public bool IsPreview { get; init; }
}
