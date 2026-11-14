using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class AcademicYearTeacher : BaseEntity
{
    private AcademicYearTeacher() { }

    internal AcademicYearTeacher(Guid teacherId)
    {
        TeacherId = teacherId;
    }

    public int AcademicYearId { get; private set; }
    public AcademicYear AcademicYear { get; private set; } = null!;

    public Guid TeacherId { get; private set; }
    public Teacher Teacher { get; private set; } = null!;
}
