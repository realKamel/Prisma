using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.PaymentAggregate;

public class RedeemCode : BaseEntity
{
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = default!;

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = default!;

    public Guid CreatedByTeacherId { get; set; }
    public Teacher CreatedByTeacher { get; set; } = default!;

    public string? Prefix { get; set; }
    public int TotalCodes { get; set; }

    public ICollection<GeneratedCode> GeneratedCodes { get; set; } = new List<GeneratedCode>();
}