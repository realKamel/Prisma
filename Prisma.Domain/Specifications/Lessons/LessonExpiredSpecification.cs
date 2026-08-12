using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonExpiredSpecification : Specification<Lesson, LessonExpiredProjection>
{
    public LessonExpiredSpecification(int lessonId, Guid studentId)
    {
        Query.Where(lesson => lesson.Id == lessonId).AsNoTracking()
       .Select(lesson => new LessonExpiredProjection
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            ImageThumbnailUrl = lesson.ImageThumbnailUrl,
            Price = lesson.Price,
            ChaptersCount = lesson.Sections.Count,
            MaterialsCount = lesson.LessonMaterials.Count,
            TeacherSubject=lesson.Teacher.Subject,  
            ExpiredDate = lesson.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.ExpiresAt)
                .FirstOrDefault(),
            TotalProgress = lesson.Sections
                .SelectMany(s => s.Progresses)
                .Where(sp => sp.StudentId == studentId)
                .Select(sp => (double?)sp.Percentage)
                .Average() ?? 0,
            Degree = lesson.Quiz != null
                ? lesson.Quiz.Attempts
                    .Where(qa => qa.StudentId == studentId
                        && qa.Status == QuizAttemptStatus.Graded
                        && qa.QuizId == lesson.Quiz.Id)
                    .Select(qa => (decimal?)qa.Degree)
                    .FirstOrDefault() ?? 0
                : 0,
            Chapters = lesson.Sections.Select(s => new ExpiredChapterProjection
            {
                Id = s.Id,
                Title = s.Title,
                Duration = s.Duration
            }).ToList()
        });

    }
}
public class LessonExpiredProjection
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageThumbnailUrl { get; set; }
    public string? TeacherSubject { get; set; } = null;
    public decimal Price { get; set; }
    public int ChaptersCount { get; set; }
    public int MaterialsCount { get; set; }
    public DateTimeOffset? ExpiredDate { get; set; }
    public double TotalProgress { get; set; }
    public decimal Degree { get; set; }
    public List<ExpiredChapterProjection> Chapters { get; set; } = [];
}

public class ExpiredChapterProjection
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public TimeSpan Duration { get; set; }
}