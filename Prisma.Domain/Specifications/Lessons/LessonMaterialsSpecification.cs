using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonMaterialsSpecification : Specification<Lesson, List<LessonMaterialProjection>>
{
    public LessonMaterialsSpecification(int lessonId)
    {
        Query
            .Where(lesson => lesson.Id == lessonId && !lesson.IsDeleted)
            .Select(lesson =>
                lesson
                    .LessonMaterials.Select(m => new LessonMaterialProjection
                    {
                        Id = m.Id,
                        Title = m.FileObject.FileName,
                        Size = m.FileObject.Size,
                        Type = m.Type.ToString(),
                        CreatedAt = m.CreatedAt,
                    })
                    .ToList()
            );
    }
}

public sealed record LessonMaterialProjection
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public long Size { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; init; }
}
