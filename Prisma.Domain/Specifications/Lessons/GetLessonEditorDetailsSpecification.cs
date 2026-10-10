using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Domain.Specifications.Lessons;

public sealed class GetLessonEditorDetailsSpecification
    : Specification<Lesson, LessonEditorDetailsProjection>
{
    public GetLessonEditorDetailsSpecification(int lessonId)
    {
        Query
            .Where(lesson => lesson.Id == lessonId)
            .AsNoTracking()
            .Select(lesson => new LessonEditorDetailsProjection
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Money = lesson.Money,
                ImageThumbnailUrl = lesson.ImageThumbnailUrl,
                PrerequisiteId = lesson.PrerequisiteId,
                Sections = lesson
                    .Sections.Select(s => new EditorSectionProjection
                    {
                        Title = s.Title,
                        ContentURL = s.ContentURL,
                        SortOrder = s.SortOrder,
                    })
                    .ToList(),
                HasAssignment = lesson.Assignment != null,
                AssignmentDueDate = lesson.Assignment != null ? lesson.Assignment.DueDate : null,
                AssignmentTitle = lesson.Assignment != null ? lesson.Assignment.Title : null,
                Outcomes = lesson.Outcomes.ToList(),
                AcademicYearIds = lesson.AcademicYears.Select(ay => ay.AcademicYearId).ToList(),
                TeacherId = lesson.TeacherId!.Value,
            });
    }
}

public record LessonEditorDetailsProjection
{
    public int Id { get; init; }
    public Guid PublicId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }

    // public decimal Price { get; init; }
    public Money Money { get; init; }
    public string? ImageThumbnailUrl { get; init; }
    public int? PrerequisiteId { get; init; }
    public IList<EditorSectionProjection> Sections { get; init; } = [];
    public bool HasAssignment { get; init; }
    public DateTimeOffset? AssignmentDueDate { get; init; }
    public string? AssignmentTitle { get; init; }
    public IList<string> Outcomes { get; init; } = [];
    public IList<int> AcademicYearIds { get; init; } = [];
    public Guid TeacherId { get; init; }
}

public record EditorSectionProjection
{
    public string? Title { get; init; }
    public string? ContentURL { get; init; }
    public int SortOrder { get; init; }
}
