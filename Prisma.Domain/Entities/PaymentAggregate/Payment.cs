using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.PaymentAggregate;

public class Payment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; }

    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }
}