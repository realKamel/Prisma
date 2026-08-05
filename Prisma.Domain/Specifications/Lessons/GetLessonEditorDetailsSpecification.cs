using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class GetLessonEditorDetailsSpecification : Specification<Lesson, LessonEditorDetailsProjection>
{
    public GetLessonEditorDetailsSpecification(int lessonId)
    {
        Query.Where(lesson => lesson.Id == lessonId).AsNoTracking()
        .Select(lesson => new LessonEditorDetailsProjection
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            Price = lesson.Price,
            ImageThumbnailUrl = lesson.ImageThumbnailUrl,
            PrerequisiteId = lesson.PrerequisiteId,
            Sections = lesson.Sections.Select(s => new EditorSectionProjection
            {
                Title = s.Title,
                ContentURL = s.ContentURL,
                SortOrder = s.SortOrder
            }).ToList(),
            HasAssignment = lesson.Assignment != null,
            AssignmentDueDate = lesson.Assignment != null ? lesson.Assignment.DueDate : null,
            AssignmentTitle = lesson.Assignment != null ? lesson.Assignment.Title : null,
            Outcomes = lesson.Outcomes.ToList(),
            AcademicYearIds = lesson.AcademicYears.Select(ay => ay.AcademicYearId).ToList()
        });

    }
}
public class LessonEditorDetailsProjection
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageThumbnailUrl { get; set; }
    public int? PrerequisiteId { get; set; }
    public List<EditorSectionProjection> Sections { get; set; } = [];
    public bool HasAssignment { get; set; }
    public DateTimeOffset? AssignmentDueDate { get; set; }
    public string? AssignmentTitle { get; set; }
    public List<string> Outcomes { get; set; } = [];
    public List<int> AcademicYearIds { get; set; } = [];
}

public class EditorSectionProjection
{
    public string? Title { get; set; }
    public string? ContentURL { get; set; }
    public int SortOrder { get; set; }
}