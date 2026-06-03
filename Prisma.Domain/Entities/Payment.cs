using System;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Payment : BaseEntity
{
    public string? StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public string? LessonId { get; set; }
    public virtual Lesson? Lesson { get; set; }

    public string? TransactionID { get; set; }
    public decimal Amount { get; set; }
    public string? Method { get; set; }
    public DateTime PaidAt { get; set; }

}