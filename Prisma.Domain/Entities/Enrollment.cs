namespace Prisma.Domain.Entities;

public class Enrollment : BaseEntity
{
    public string? EnrollmentMethod { get; set; }
    public DateTimeOffset? ExDate { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
}