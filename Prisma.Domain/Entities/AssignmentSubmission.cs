using System;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class AssignmentSubmission : BaseEntity
{
    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }

    public int? AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public string? FileURL { get; set; }
    public DateTime SubmittedAt { get; set; }
}