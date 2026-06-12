using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.EnrollmentAggregate;

public class Report : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; }
}