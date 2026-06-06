using Prisma.Domain.Common;

namespace Prisma.Domain.Entities;

public class Report : BaseEntity
{
    public string? Content { get; set; }
    public DateTimeOffset Date { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
}