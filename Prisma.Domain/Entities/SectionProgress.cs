using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class SectionProgress : BaseEntity
{
    public bool IsCompleted { get; set; }

    public string? StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public string? SectionId { get; set; }
    public virtual Section? Section { get; set; }
}