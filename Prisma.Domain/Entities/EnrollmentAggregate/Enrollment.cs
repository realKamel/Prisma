using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.ValueObjects.EnrollmentDomain;

namespace Prisma.Domain.Entities.EnrollmentAggregate;

public class Enrollment : BaseEntity
{
    public Guid? PublicId { get; init; } = Guid.CreateVersion7();
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public EnrollmentMethod EnrollmentMethod { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public DateRange? ValidityDateRange { get; set; }
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }

    public Payment? Payment { get; set; }
    public int? PaymentId { get; set; }

    public int? GeneratedCodeId { get; set; } // set when EnrollmentMethod == RedeemCode
    public GeneratedCode? GeneratedCode { get; set; }

    public void CompleteEnrollment()
    {
        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
        Status = EnrollmentStatus.Done;
    }

    // Private constructor for EF Core hydration
    // private Enrollment() { }

    /// <summary>
    /// Factory method to validate domain rules and construct an Enrollment aggregate.
    /// </summary>
    public static Result<Enrollment> Create(
        Guid studentId,
        int lessonId,
        EnrollmentMethod method,
        int? paymentId = null,
        int? generatedCodeId = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        var validationErrors = new List<ValidationError>();

        // 1. Validate mandatory references
        if (studentId == Guid.Empty)
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = nameof(StudentId), ErrorMessage = "StudentId is required to create an enrollment."
            });
        }

        if (lessonId <= 0)
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = nameof(LessonId), ErrorMessage = "A valid LessonId is required."
            });
        }

        // 2. Validate Method-Specific Domain Rules
        switch (method)
        {
            case EnrollmentMethod.RedeemCode when generatedCodeId is not > 0:
                validationErrors.Add(new ValidationError
                {
                    Identifier = nameof(GeneratedCodeId),
                    ErrorMessage = "GeneratedCodeId is required when EnrollmentMethod is RedeemCode."
                });
                break;
            case EnrollmentMethod.OnlinePayment when paymentId is not > 0:
                validationErrors.Add(new ValidationError
                {
                    Identifier = nameof(PaymentId),
                    ErrorMessage = "PaymentId is required when EnrollmentMethod is Payment."
                });
                break;
            case EnrollmentMethod.TeacherGrant:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }

        // 3. Handle DateRange Value Object Creation (if dates are supplied)
        DateRange? validityDateRange = null;
        if (startDate.HasValue && endDate.HasValue)
        {
            var dateRangeResult = DateRange.Create(startDate.Value, endDate.Value);

            if (!dateRangeResult.IsSuccess)
            {
                validationErrors.AddRange(dateRangeResult.ValidationErrors);
            }
            else
            {
                validityDateRange = dateRangeResult.Value;
            }
        }

        if (validationErrors.Count > 0)
        {
            return Result.Invalid(validationErrors);
        }

        // 4. Construct Aggregate Root
        var enrollment = new Enrollment
        {
            PublicId = Guid.CreateVersion7(),
            StudentId = studentId,
            LessonId = lessonId,
            EnrollmentMethod = method,
            PaymentId = paymentId,
            GeneratedCodeId = generatedCodeId,
            ValidityDateRange = validityDateRange,
            ExpiresAt = endDate,
            Status = EnrollmentStatus.Active,
            IsCompleted = false
        };

        return Result.Success(enrollment);
    }


    //TODO:Why not ? 
    // public void CancelEnrollment()
    // {
    //     Status = EnrollmentStatus.Cancelled;
    // }
}