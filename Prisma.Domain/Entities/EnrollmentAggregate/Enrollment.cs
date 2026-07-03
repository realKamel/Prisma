using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.EnrollmentAggregate;

public class Enrollment : BaseEntity
{
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public EnrollmentMethod EnrollmentMethod { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }

    public Payment? Payment { get; set; }
    public int? PaymentId { get; set; }

    public int? GeneratedCodeId { get; set; } // set when EnrollmentMethod == RedeemCode
    public GeneratedCode? GeneratedCode { get; set; }
}