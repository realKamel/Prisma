using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class SectionProgress : BaseEntity
{
    public bool IsCompleted { get; set; }

    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }

    public int? SectionId { get; set; }
    public Section? Section { get; set; }
}