using Prisma.Domain.Common;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.LessonAggregate;

public class LessonMaterial : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public LessonMaterialType Type { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }
}