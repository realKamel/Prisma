using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonPlayerWithDetailsSpecification : Specification<Lesson, LessonPlayerProjection>
{
    public LessonPlayerWithDetailsSpecification(int lessonId, Guid studentId)
    {
        Query.Where(lesson => lesson.Id == lessonId).AsNoTracking().

       Select(lesson => new LessonPlayerProjection
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            ImageThumbnailUrl = lesson.ImageThumbnailUrl,
            Subject = lesson.Teacher.Subject,
            TeacherName = lesson.Teacher.FirstName + " " + lesson.Teacher.LastName,
            Outcomes = lesson.Outcomes.ToList(),

            EnrollmentExpiresAt = lesson.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.ExpiresAt)
                .FirstOrDefault(),

            Sections = lesson.Sections.Select(s => new PlayerSectionProjection
            {
                Id = s.Id,
                SortOrder = s.SortOrder,
                Title = s.Title,
                Duration = s.Duration,
                PlaybackId = s.PlaybackId,
                IsCompleted = s.Progresses
                    .Where(p => p.StudentId == studentId)
                    .Select(p => (bool?)p.IsCompleted)
                    .FirstOrDefault() ?? false,
                WatchedSeconds = s.Progresses
                    .Where(p => p.StudentId == studentId)
                    .Select(p => (int?)p.WatchedSeconds)
                    .FirstOrDefault() ?? 0
            }).ToList(),

            Materials = lesson.LessonMaterials.Select(m => new PlayerMaterialProjection
            {
                Title = m.Title,
                DownloadUrl = m.DownloadUrl,
                Type = (int)m.Type
            }).ToList(),

            Quiz = lesson.Quiz != null ? new PlayerQuizProjection
            {
                Id = lesson.Quiz.Id,
                QuestionsCount = lesson.Quiz.Questions.Count,
                TimeInMinutes = lesson.Quiz.TimeInMinutes,
                TotalDegree = lesson.Quiz.TotalDegree,
                IsAttempted = lesson.Quiz.Attempts.Any(a => a.StudentId == studentId)
            } : null,

            Assignment = lesson.Assignment != null ? new PlayerAssignmentProjection
            {
                Id = lesson.Assignment.Id,
                ContentURL = lesson.Assignment.ContentURL,
                DueDate = lesson.Assignment.DueDate,
                SubmissionTitle = lesson.Assignment.Submissions
                    .Where(sub => sub.StudentId == studentId)
                    .Select(sub => sub.Title)
                    .FirstOrDefault()
            } : null
        });

      
    }
}


public class LessonPlayerProjection
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageThumbnailUrl { get; set; }
    public string? Subject { get; set; }
    public string? TeacherName { get; set; }
    public List<string> Outcomes { get; set; } = [];
    public DateTimeOffset? EnrollmentExpiresAt { get; set; }

    public List<PlayerSectionProjection> Sections { get; set; } = [];
    public List<PlayerMaterialProjection> Materials { get; set; } = [];
    public PlayerQuizProjection? Quiz { get; set; }
    public PlayerAssignmentProjection? Assignment { get; set; }
}

public class PlayerSectionProjection
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string? Title { get; set; }
    public TimeSpan Duration { get; set; }
    public string? PlaybackId { get; set; }
    public bool IsCompleted { get; set; }
    public int WatchedSeconds { get; set; }
}

public class PlayerMaterialProjection
{
    public string? Title { get; set; }
    public string? DownloadUrl { get; set; }
    public int Type { get; set; }
}

public class PlayerQuizProjection
{
    public int Id { get; set; }
    public int QuestionsCount { get; set; }
    public TimeSpan TimeInMinutes { get; set; }
    public decimal TotalDegree { get; set; }
    public bool IsAttempted { get; set; }
}

public class PlayerAssignmentProjection
{
    public int Id { get; set; }
    public string? ContentURL { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public string? SubmissionTitle { get; set; }
}