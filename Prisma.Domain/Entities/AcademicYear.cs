using Prisma.Domain.Common;

namespace Prisma.Domain.Entities;

public class AcademicYear : BaseEntity
{
    public required string Title { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Teacher> Teachers{ get; set; } = new List<Teacher>();
    public ICollection<Lesson> Lessons{ get; set; } = new List<Lesson>();
}
