using Prisma.Domain.Common;

namespace Prisma.Domain.Entities;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public string? Method { get; set; }

    public DateTimeOffset PaidAt { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public string? TransactionID { get; set; }
}