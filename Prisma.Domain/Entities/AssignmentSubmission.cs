using System;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class AssignmentSubmission : BaseEntity
{
    public string? StudentId { get; set; }
    public virtual Student? Student { get; set; }

    public string? AssignmentId { get; set; } 
    public virtual Assignment? Assignment { get; set; }

    public string? FileURL { get; set; }
    public DateTime SubmittedAt { get; set; }

}