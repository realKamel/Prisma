using Prisma.Domain.Common;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.EnrollmentAggregate;

public class Enrollment : BaseEntity
{
    public EnrollmentMethod? EnrollmentMethod { get; set; }
    public DateTimeOffset? ExDate { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
}