using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonMaterialsSpecification : Specification<Lesson, List<LessonMaterialProjection>>
{
    public LessonMaterialsSpecification(int lessonId)
    {
        Query.Where(lesson => lesson.Id == lessonId && !lesson.IsDeleted)
            .Select(lesson => lesson.LessonMaterials.Select(m => new LessonMaterialProjection
        {
            Id = m.Id,
            Title = m.Title,
            Size = m.Size,
            Type = m.Type.ToString(),
            CreatedAt = m.CreatedAt
        }).ToList());
    }
}
public class LessonMaterialProjection
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string Size { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
}