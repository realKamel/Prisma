using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.PaymentAggregate;

public class Payment : BaseEntity
{
    public string Provider { get; set; } = default!; // "Fawry" | "Paymob"
    public string ProviderRef { get; set; } = default!; // external order/intent ID

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "EGP"; // ISO 4217

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTimeOffset? PaidAt { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; }

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }
}