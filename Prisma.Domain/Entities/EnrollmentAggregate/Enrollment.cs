using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.EnrollmentAggregate;

public class Enrollment : BaseEntity
{
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public EnrollmentMethod EnrollmentMethod { get; set; } // 

    public DateTimeOffset? ExpiresAt { get; set; }

    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }

    public Payment? Payment { get; set; }
    public int? PaymentId { get; set; }

    public Guid? RedeemCodeId { get; set; } // set when GrantType == RedeemCode
    public RedeemCode? RedeemCode { get; set; }
}