using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonWithDetailsSpecification : Specification<Lesson, LessonDetailsProjection>
{
    public LessonWithDetailsSpecification(int lessonId)
    {
        Query
            .Where(lesson => lesson.Id == lessonId).AsNoTracking()
            .Select(lesson => new LessonDetailsProjection
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Price = lesson.Price,
                ImageThumbnailUrl = lesson.ImageThumbnailUrl,
                EnrollmentsCount = lesson.Enrollments.Count,
                TeacherName = lesson.Teacher.FirstName + " " + lesson.Teacher.LastName,
                TeacherSubject = lesson.Teacher.Subject,
                Outcomes = lesson.Outcomes.ToList(),
                Sections = lesson.Sections.Select(s => new SectionProjection
                {
                    Id = s.Id,
                    Title = s.Title,
                    Duration = s.Duration,
                    IsPreview = s.IsPreview
                }).ToList(),
                PrerequisiteId = lesson.Prerequisite != null ? lesson.Prerequisite.Id : (int?)null,
                PrerequisiteTitle = lesson.Prerequisite != null ? lesson.Prerequisite.Title : null
            });
    } }
    public class LessonDetailsProjection
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public int EnrollmentsCount { get; set; }
        public List<string> Outcomes { get; set; } = [];
        public List<SectionProjection> Sections { get; set; } = [];
        public int? PrerequisiteId { get; set; }
        public string? PrerequisiteTitle { get; set; }
    public string TeacherName { get; set; }
    public string TeacherSubject { get; set; }
}

    public class SectionProjection
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsPreview { get; set; }
    }

