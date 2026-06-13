using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.LessonAggregate;

public class Section : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? ContentURL { get; set; }

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public ICollection<SectionProgress> Progresses { get; set; } = new List<SectionProgress>();

    public int? DurationInMinutes { get; set; }  
    
    public bool IsPreview { get; set; } = false; // معاينه مجانيه

}