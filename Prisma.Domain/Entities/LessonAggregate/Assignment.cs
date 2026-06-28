using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.LessonAggregate;

public class Assignment : BaseEntity
{
    public string? ContentURL { get; set; }
    public DateTimeOffset DueDate { get; set; }

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }
    public int Grade {set; get;}

    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}