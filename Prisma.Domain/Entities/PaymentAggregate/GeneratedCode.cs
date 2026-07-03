using Prisma.Domain.Common;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.PaymentAggregate;

// A single redeemable code belonging to a RedeemCode batch.
public class GeneratedCode : BaseEntity
{
    public int BatchId { get; set; }
    public RedeemCode Batch { get; set; } = default!;

    public string Code { get; set; } = default!;

    public Guid? RedeemedByStudentId { get; set; }
    public Student? RedeemedByStudent { get; set; }
    public DateTimeOffset? RedeemedAt { get; set; }

    public Enrollment? Enrollment { get; set; }
}