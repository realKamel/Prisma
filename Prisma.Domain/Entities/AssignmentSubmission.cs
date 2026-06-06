using Prisma.Domain.Common;

namespace Prisma.Domain.Entities;

public class AssignmentSubmission : BaseEntity
{
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public string? FileUrl { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
}