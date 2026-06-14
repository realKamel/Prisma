using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Entities.PaymentAggregate;

public class RedeemCode : BaseEntity
{
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public string Code { get; set; } = default!;
    public int MaxUses { get; set; } = 1;
    public int UsedCount { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public Guid? RedeemedByStudentId { get; set; } // set only when MaxUses == 1
    public DateTimeOffset? RedeemedAt { get; set; }
}