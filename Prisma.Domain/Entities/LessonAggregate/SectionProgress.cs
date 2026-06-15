using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public class SectionProgress : BaseEntity
{
    public bool IsCompleted { get; set; }
    public int Percentage { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; }

    public int SectionId { get; set; }
    public Section Section { get; set; }
}